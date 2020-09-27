module Slp.Evi.Relational.RelationalAlgebraBuilder

open System
open FSharpx.Collections

open Slp.Evi.Common
open Slp.Evi.Common.Types
open Slp.Evi.R2RML
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Relational.Algebra
open Slp.Evi.Relational.RelationalAlgebraOptimizers
open Slp.Evi.R2RML.MappingTemplate
open Slp.Evi.Common.Algebra
open Slp.Evi.Common.Database

let rec private isBaseValueBinderBoundCondition neededVars =
    if neededVars |> Map.isEmpty then
        AlwaysTrue |> optimizeRelationalCondition
    else
        neededVars
        |> Map.toList
        |> List.map snd
        |> List.distinct
        |> List.map (fun v -> v |> IsNull |> optimizeRelationalCondition |> Not |> optimizeRelationalCondition)
        |> Conjunction |> optimizeRelationalCondition

let rec private valueBinderToExpressionSet (typeIndexer: TypeIndexer) valueBinder =
    match valueBinder with
    | EmptyValueBinder ->
        ExpressionSet.empty
    | BaseValueBinder(mapping, neededVariables) ->
        let isBoundCondition = isBaseValueBinderBoundCondition neededVariables
        match mapping with
        | IriObject iriMapping ->
            let nodeTypeRecord =
                if iriMapping.IsBlankNode then BlankNodeType else IriNodeType
                |> typeIndexer.FromType

            let expression =
                match iriMapping.Value with
                | IriColumn c -> neededVariables |> Map.find c.Name |> Variable |> optimizeRelationalExpression
                | IriConstant i -> i |> Iri.toText |> String |> Constant |> optimizeRelationalExpression
                | IriTemplate parts ->
                    parts
                    |> List.map (
                        function
                        | TemplatePart.TextPart t -> t |> String |> Constant |> optimizeRelationalExpression
                        | TemplatePart.ColumnPart c -> neededVariables |> Map.find c.Name |> IriSafeVariable |> optimizeRelationalExpression
                    )
                    |> Concatenation
                    |> optimizeRelationalExpression

            { ExpressionSet.empty with
                IsNotErrorCondition = isBoundCondition
                TypeCategoryExpression = nodeTypeRecord.Category |> int |> Int |> Constant |> optimizeRelationalExpression
                TypeExpression = nodeTypeRecord.Index |> Int |> Constant |> optimizeRelationalExpression
                StringExpression = expression
            }
            
        | LiteralObject literalMapping ->
            let nodeTypeRecord =
                literalMapping.Type
                |> LiteralNodeType
                |> typeIndexer.FromType

            let expression =
                match literalMapping.Value with
                | LiteralColumn c -> neededVariables |> Map.find c.Name |> Variable |> optimizeRelationalExpression
                | LiteralConstant s -> s |> String |> Constant |> optimizeRelationalExpression
                | LiteralTemplate parts ->
                    parts
                    |> List.map (
                        function
                        | TemplatePart.TextPart t -> t |> String |> Constant |> optimizeRelationalExpression
                        | TemplatePart.ColumnPart c -> neededVariables |> Map.find c.Name |> Variable |> optimizeRelationalExpression
                    )
                    |> Concatenation
                    |> optimizeRelationalExpression

            let baseRecord =
                { ExpressionSet.empty with
                    IsNotErrorCondition = isBoundCondition
                    TypeCategoryExpression = nodeTypeRecord.Category |> int |> Int |> Constant |> optimizeRelationalExpression
                    TypeExpression = nodeTypeRecord.Index |> Int |> Constant |> optimizeRelationalExpression
                }

            match nodeTypeRecord.Category with
            | TypeIndexer.TypeCategory.BooleanLiteral ->
                { baseRecord with
                    BooleanExpression = expression
                }
            | TypeIndexer.TypeCategory.NumericLiteral ->
                { baseRecord with
                    NumericExpression = expression
                }
            | TypeIndexer.TypeCategory.DateTimeLiteral ->
                { baseRecord with
                    DateTimeExpresion = expression
                }
             | _ ->
                { baseRecord with
                    StringExpression = expression
                }

    | CaseValueBinder(caseVariable, cases) ->
        let transformedCases =
            cases
            |> Map.map (fun _ vb -> vb |> valueBinderToExpressionSet typeIndexer)

        let buildExpression selector =
            transformedCases
            |> Map.toList
            |> List.map (
                fun (cv, vb) ->
                    {
                        Condition = EqualVariableTo(caseVariable, cv |> Int) |> optimizeRelationalCondition
                        Expression = vb |> selector
                    }
            )
            |> Switch |> optimizeRelationalExpression

        {
            IsNotErrorCondition =
                transformedCases
                |> Map.toList
                |> List.map (
                    fun (cv, vb) ->
                        [
                            EqualVariableTo(caseVariable, cv |> Int) |> optimizeRelationalCondition
                            vb.IsNotErrorCondition
                        ] |> Conjunction |> optimizeRelationalCondition
                ) |> Disjunction |> optimizeRelationalCondition
            TypeCategoryExpression = buildExpression (fun x -> x.TypeCategoryExpression)
            TypeExpression = buildExpression (fun x -> x.TypeExpression)
            StringExpression = buildExpression (fun x -> x.StringExpression)
            NumericExpression = buildExpression (fun x -> x.NumericExpression)
            BooleanExpression = buildExpression (fun x -> x.BooleanExpression)
            DateTimeExpresion = buildExpression (fun x -> x.DateTimeExpresion)
        }

    | CoalesceValueBinder valueBinders ->
        let transformed = valueBinders |> List.map (valueBinderToExpressionSet typeIndexer)
        let buildExpression selector =
            let rec buildExpressionImpl condition result rest =
                match rest with
                | [] -> result |> List.rev |> Switch |> optimizeRelationalExpression
                | x :: xs ->
                    let isBound = [ condition; x.IsNotErrorCondition ] |> Conjunction |> optimizeRelationalCondition
                    let nextCondition = [ condition; x.IsNotErrorCondition |> Not |> optimizeRelationalCondition ] |> Conjunction |> optimizeRelationalCondition
                    let newResult = { Condition = isBound; Expression = x |> selector } :: result
                    xs |> buildExpressionImpl nextCondition newResult
            buildExpressionImpl AlwaysTrue List.empty transformed

        {
            IsNotErrorCondition =
                transformed
                |> List.map (fun vb -> vb.IsNotErrorCondition)
                |> Conjunction
                |> optimizeRelationalCondition
            TypeCategoryExpression = buildExpression (fun x -> x.TypeCategoryExpression)
            TypeExpression = buildExpression (fun x -> x.TypeExpression)
            StringExpression = buildExpression (fun x -> x.StringExpression)
            NumericExpression = buildExpression (fun x -> x.NumericExpression)
            BooleanExpression = buildExpression (fun x -> x.BooleanExpression)
            DateTimeExpresion = buildExpression (fun x -> x.DateTimeExpresion)
        }

    | ExpressionValueBinder expressionSet -> expressionSet

let private nodeToExpressionSet (typeIndexer: TypeIndexer) node =
    match node with
    | IriNode iriNode ->
        let nodeTypeRecord =
            if iriNode.IsBlankNode then BlankNodeType else IriNodeType
            |> typeIndexer.FromType

        { ExpressionSet.empty with
            IsNotErrorCondition = AlwaysTrue
            TypeCategoryExpression = nodeTypeRecord.Category |> int |> Int |> Constant |> optimizeRelationalExpression
            TypeExpression = nodeTypeRecord.Index |> Int |> Constant |> optimizeRelationalExpression
            StringExpression = iriNode.Iri |> Iri.toText |> String |> Constant |> optimizeRelationalExpression
        }

    | LiteralNode literalNode ->
        let nodeTypeRecord =
            literalNode.ValueType
            |> LiteralNodeType
            |> typeIndexer.FromType

        let baseRecord =
            { ExpressionSet.empty with
                IsNotErrorCondition = AlwaysTrue
                TypeCategoryExpression = nodeTypeRecord.Category |> int |> Int |> Constant |> optimizeRelationalExpression
                TypeExpression = nodeTypeRecord.Index |> Int |> Constant |> optimizeRelationalExpression
            }

        match nodeTypeRecord.Category with
        | TypeIndexer.TypeCategory.BooleanLiteral ->
            NotImplementedException("BooleanLiteral conversion not yet implemented")
            |> raise
        | TypeIndexer.TypeCategory.NumericLiteral ->
            let numericLiteral =
                match Int32.TryParse(literalNode.Value) with
                | true, intValue -> intValue |> Int
                | false, _ ->
                    match System.Double.TryParse(literalNode.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture) with
                    | true, doubleValue -> doubleValue |> Double
                    | false, _ ->
                        sprintf "Failed to parse numeric: %s" literalNode.Value
                        |> invalidOp

            { baseRecord with
                NumericExpression = numericLiteral |> Constant
            }
        | TypeIndexer.TypeCategory.DateTimeLiteral ->
            let dateTimeLiteral =
                System.Xml.XmlConvert.ToDateTime(literalNode.Value, System.Xml.XmlDateTimeSerializationMode.Utc)
                |> DateTimeLiteral

            { baseRecord with
                DateTimeExpresion = dateTimeLiteral |> Constant
            }
         | _ ->
            { baseRecord with
                StringExpression = literalNode.Value |> String |> Constant
            }

let private isExpressionSetInCategory (category: TypeIndexer.TypeCategory) exprSet =
    Comparison(Comparisons.EqualTo, exprSet.TypeCategoryExpression, category |> int |> Int |> Constant)
    |> optimizeRelationalCondition

let private isExpressionSetInOneOfCategories (categories: TypeIndexer.TypeCategory list) exprSet =
    categories
    |> List.map (fun x -> isExpressionSetInCategory x exprSet)
    |> List.map optimizeRelationalCondition
    |> Disjunction
    |> optimizeRelationalCondition

let private expressionSetValueUncheckedComparison comparison left right =
    let notIsValueInCategory category =
        isExpressionSetInCategory category left
        |> Not
        |> optimizeRelationalCondition

    let notIsValueNumeric = notIsValueInCategory TypeIndexer.TypeCategory.NumericLiteral
    let notIsValueBoolean = notIsValueInCategory TypeIndexer.TypeCategory.BooleanLiteral
    let notIsValueDateTime = notIsValueInCategory TypeIndexer.TypeCategory.DateTimeLiteral
    let notIsValueString =
        [
            TypeIndexer.TypeCategory.BlankNode
            TypeIndexer.TypeCategory.Iri
            TypeIndexer.TypeCategory.StringLiteral
            TypeIndexer.TypeCategory.SimpleLiteral
            TypeIndexer.TypeCategory.OtherLiteral
        ]
        |> List.map notIsValueInCategory
        |> Conjunction |> optimizeRelationalCondition

    let expressionComparison selector =
        Comparison(comparison, left |> selector, right |> selector)
        |> optimizeRelationalCondition

    [
        Comparison(Comparisons.EqualTo, left.TypeCategoryExpression, right.TypeCategoryExpression)
        [ notIsValueString; expressionComparison (fun x -> x.StringExpression) ] |> Disjunction
        [ notIsValueNumeric; expressionComparison (fun x -> x.NumericExpression) ] |> Disjunction
        [ notIsValueBoolean; expressionComparison (fun x -> x.BooleanExpression) ] |> Disjunction
        [ notIsValueDateTime; expressionComparison (fun x -> x.DateTimeExpresion) ] |> Disjunction
    ]
    |> List.map optimizeRelationalCondition
    |> Conjunction
    |> optimizeRelationalCondition

let private expressionSetValuesEqualCondition left right =
    [
        Comparison(Comparisons.EqualTo, left.TypeExpression, right.TypeExpression) |> optimizeRelationalCondition
        expressionSetValueUncheckedComparison Comparisons.EqualTo left right
    ]
    |> Conjunction
    |> optimizeRelationalCondition

let private valueBinderValueEqualToNodeCondition typeIndexer valueBinder node =
    (valueBinder |> valueBinderToExpressionSet typeIndexer, node |> nodeToExpressionSet typeIndexer)
    ||> expressionSetValuesEqualCondition

let private valueBindersEqualValueCondition typeIndexer valueBinder otherValueBinder =
    (valueBinder |> valueBinderToExpressionSet typeIndexer, otherValueBinder |> valueBinderToExpressionSet typeIndexer)
    ||> expressionSetValuesEqualCondition

let rec private processSparqlExpression (typeIndexer: TypeIndexer) (bindings: Map<SparqlVariable, ValueBinder>) (expression: SparqlExpression): ExpressionSet =
    match expression with
    | BooleanExpression(sparqlCondition) ->
        let (isNonError, condition) = sparqlCondition |> processSparqlCondition typeIndexer bindings
        { ExpressionSet.empty with
            IsNotErrorCondition = isNonError
            TypeCategoryExpression = TypeIndexer.TypeCategory.BooleanLiteral |> int |> Int |> Constant |> optimizeRelationalExpression
            TypeExpression = typeIndexer.FromType (KnownTypes.xsdBoolean |> LiteralNodeType) |> fun x -> x.Index |> Int |> Constant |> optimizeRelationalExpression
            BooleanExpression = condition |> Boolean |> optimizeRelationalExpression
        }
        
    | NodeExpression(node) ->
        node |> nodeToExpressionSet typeIndexer

    | VariableExpression(sparqlVariable) ->
        match bindings.TryGetValue sparqlVariable with
        | true, binder ->
            binder |> valueBinderToExpressionSet typeIndexer
        | false, _ ->
            ExpressionSet.empty

    | BinaryArithmeticExpression(operator, left, right) ->
        let leftProc = left |> processSparqlExpression typeIndexer bindings
        let rightProc = right |> processSparqlExpression typeIndexer bindings
        let possibleNumericTypes =
            typeIndexer.IndexedTypes
            |> List.filter (fun x -> x.Category = TypeIndexer.TypeCategory.NumericLiteral)

        let isNotError =
            [
                leftProc.IsNotErrorCondition
                rightProc.IsNotErrorCondition
                leftProc |> isExpressionSetInCategory TypeIndexer.TypeCategory.NumericLiteral
                rightProc |> isExpressionSetInCategory TypeIndexer.TypeCategory.NumericLiteral
            ]
            |> Conjunction
            |> optimizeRelationalCondition

        let typeExpression =
            possibleNumericTypes
            |> List.collect (
                fun leftType ->
                    possibleNumericTypes
                    |> List.map (
                        fun rightType ->
                            let finalType =
                                match leftType.NodeType, rightType.NodeType with
                                | LiteralNodeType lt, LiteralNodeType rt ->
                                    if lt = KnownTypes.xsdDouble || rt = KnownTypes.xsdDouble then
                                        KnownTypes.xsdDouble
                                    elif lt = KnownTypes.xsdDecimal || rt = KnownTypes.xsdDecimal then
                                        KnownTypes.xsdDecimal
                                    elif lt = KnownTypes.xsdInteger && rt = KnownTypes.xsdInteger then
                                        KnownTypes.xsdInteger
                                    else
                                        sprintf "Unsupported numeric types for arithmetic expression: %A; %A" lt rt
                                        |> invalidOp
                                | _ ->
                                    sprintf "Unsupported non-literal types for arithmetic expression: %A; %A" leftType.NodeType rightType.NodeType
                                    |> invalidOp
                                |> LiteralNodeType
                                |> typeIndexer.FromType

                            (finalType, (leftType, rightType))
                    )
            )
            |> List.groupBy fst
            |> List.map (
                fun (finalType, combs) ->
                    combs
                    |> List.map snd
                    |> List.distinct
                    |> fun x -> finalType, x
            )
            |> List.map (
                fun (finalType, combs) ->
                    let condition =
                        combs
                        |> List.map (
                            fun (leftType, rightType) ->
                                [
                                    Comparison(Comparisons.EqualTo, leftProc.TypeExpression, leftType.Index |> Int |> Constant) |> optimizeRelationalCondition
                                    Comparison(Comparisons.EqualTo, rightProc.TypeExpression, rightType.Index |> Int |> Constant) |> optimizeRelationalCondition
                                ]
                                |> Conjunction
                                |> optimizeRelationalCondition
                        )
                        |> Disjunction
                        |> optimizeRelationalCondition

                    { Condition = condition; Expression = finalType.Index |> Int |> Constant }
            )
            |> Switch
            |> optimizeRelationalExpression

        { ExpressionSet.empty with
            IsNotErrorCondition = isNotError
            TypeCategoryExpression = TypeIndexer.TypeCategory.NumericLiteral |> int |> Int |> Constant
            TypeExpression = typeExpression
            NumericExpression = BinaryNumericOperation(operator, leftProc.NumericExpression, rightProc.NumericExpression)
        }

    | LangExpression(inner) ->
        let procInner = inner |> processSparqlExpression typeIndexer bindings

        { ExpressionSet.empty with
            IsNotErrorCondition = procInner.IsNotErrorCondition
            TypeCategoryExpression = TypeIndexer.TypeCategory.SimpleLiteral |> int |> Int |> Constant |> optimizeRelationalExpression
            TypeExpression = DefaultType |> LiteralNodeType |> typeIndexer.FromType |> fun x -> x.Index |> Int |> Constant |> optimizeRelationalExpression
            StringExpression =
                typeIndexer.IndexedTypes
                |> List.map (
                    fun indexedType ->
                        let condition =
                            Comparison(Comparisons.EqualTo, procInner.TypeExpression, indexedType.Index |> Int |> Constant)
                            |> optimizeRelationalCondition

                        let lang =
                            match indexedType.NodeType with
                            | LiteralNodeType(WithLanguage(l)) -> l
                            | _ -> String.Empty
                            |> String
                            |> Constant
                            |> optimizeRelationalExpression

                        { Condition = condition; Expression = lang }
                )
                |> Switch
                |> optimizeRelationalExpression
        }

and private processSparqlCondition (typeIndexer: TypeIndexer) (bindings: Map<SparqlVariable, ValueBinder>) (condition: SparqlCondition) =
    (fun (isNonError, condition) ->
        isNonError |> optimizeRelationalCondition, condition |> optimizeRelationalCondition
    ) <|
    match condition with
    | AlwaysFalseCondition ->
        AlwaysTrue, AlwaysFalse

    | AlwaysTrueCondition ->
        AlwaysTrue, AlwaysTrue

    | ConjunctionCondition inners ->
        let (isNonErrors, conditions) =
            inners
            |> List.map (processSparqlCondition typeIndexer bindings)
            |> List.unzip
        isNonErrors |> Conjunction, conditions |> Conjunction

    | DisjunctionCondition inners ->
        let (isNonErrors, conditions) =
            inners
            |> List.map (processSparqlCondition typeIndexer bindings)
            |> List.unzip
        isNonErrors |> Conjunction, conditions |> Disjunction

    | IsBoundCondition variable ->
        match bindings.TryGetValue variable with
        | true, valueBinder ->
            AlwaysTrue, valueBinder |> valueBinderToExpressionSet typeIndexer |> fun x -> x.IsNotErrorCondition
        | false, _ ->
            AlwaysTrue, AlwaysFalse

    | NegationCondition inner ->
        inner
        |> processSparqlCondition typeIndexer bindings
        |> fun (isNonError, condition) -> isNonError, condition |> Not

    | ComparisonCondition(comp, left, right) ->
        let procLeft = left |> processSparqlExpression typeIndexer bindings
        let procRight = right |> processSparqlExpression typeIndexer bindings

        let isNonTypeError =
            match comp with
            | Comparisons.EqualTo -> List.empty
            | _ ->
                [
                    Comparison(Comparisons.EqualTo, procLeft.TypeCategoryExpression, procRight.TypeCategoryExpression) |> optimizeRelationalCondition
                    [
                        TypeIndexer.TypeCategory.SimpleLiteral
                        TypeIndexer.TypeCategory.StringLiteral
                        TypeIndexer.TypeCategory.BooleanLiteral
                        TypeIndexer.TypeCategory.DateTimeLiteral
                        TypeIndexer.TypeCategory.NumericLiteral
                    ] |> isExpressionSetInOneOfCategories <| procLeft
                ]

        let isNonError =
            procLeft.IsNotErrorCondition :: procRight.IsNotErrorCondition :: isNonTypeError
            |> Conjunction

        let exactTypeEquality =
            match comp with
            | Comparisons.EqualTo ->
                [
                    Comparison(Comparisons.EqualTo, procLeft.TypeExpression, procRight.TypeExpression) |> optimizeRelationalCondition
                    expressionSetValuesEqualCondition procLeft procRight
                ]
                |> Conjunction
                |> optimizeRelationalCondition
                |> List.singleton
            | _ -> List.empty

        let nonExactTypeEquality =
            expressionSetValueUncheckedComparison comp procLeft procRight

        isNonError, nonExactTypeEquality :: exactTypeEquality |> Disjunction |> optimizeRelationalCondition

    | LanguageMatchesCondition langMatch ->
        let langExprSet = langMatch.Language |> processSparqlExpression typeIndexer bindings
        let langRangeExprSet = langMatch.LanguageRange |> processSparqlExpression typeIndexer bindings
        [
            langExprSet.IsNotErrorCondition
            langRangeExprSet.IsNotErrorCondition
            langExprSet |> isExpressionSetInCategory TypeIndexer.TypeCategory.SimpleLiteral
            langRangeExprSet |> isExpressionSetInCategory TypeIndexer.TypeCategory.SimpleLiteral
        ]
        |> Conjunction, LanguageMatch(langExprSet.StringExpression, langRangeExprSet.StringExpression)

    | RegexCondition regex ->
        if regex.Flags.IsSome then "Regex flags are not yet implemented" |> NotImplementedException |> raise
        else
            let (pattern, producesError) =
                match regex.Pattern with
                | NodeExpression(LiteralNode({ Value = value; ValueType = DefaultType})) ->
                    value, false
                | NodeExpression(LiteralNode({ Value = value; ValueType = _})) ->
                    value, true
                | _ ->
                    sprintf "The following regex pattern is not supported: %A" regex.Pattern
                    |> invalidOp

            let processedExpression = regex.Expression |> processSparqlExpression typeIndexer bindings

            // TODO: Improve this function
            let createLikePattern regexPattern =
                let sb = System.Text.StringBuilder()
                let rec implCreateLikePattern = function
                    | [] ->
                        sb.Append '%' |> ignore
                        sb.ToString ()
                    | ['$'] ->
                        sb.ToString ()
                    | '%' :: xs ->
                        sb.Append "[%]" |> ignore
                        implCreateLikePattern xs
                    | '[' :: _
                    | ']' :: _ ->
                        sprintf "The complex regex patterns are not yet supported: %s" pattern
                        |> invalidOp
                    | '_' :: xs ->
                        sb.Append "[_]" |> ignore
                        implCreateLikePattern xs
                    | x :: xs ->
                        sb.Append x |> ignore
                        implCreateLikePattern xs

                match regexPattern with
                | '^' :: xs ->
                    implCreateLikePattern xs
                | xs ->
                    sb.Append '%' |> ignore
                    implCreateLikePattern xs

            let likePattern = pattern |> Seq.toList |> createLikePattern

            [
                processedExpression.IsNotErrorCondition
                [
                   processedExpression |> isExpressionSetInCategory TypeIndexer.TypeCategory.StringLiteral
                   processedExpression |> isExpressionSetInCategory TypeIndexer.TypeCategory.SimpleLiteral
                ] |> Disjunction |> optimizeRelationalCondition
                if producesError then AlwaysFalse else AlwaysTrue
            ] |> Conjunction, Like (processedExpression.StringExpression, likePattern)

let private processRestrictedTriplePattern (typeIndexer: TypeIndexer) (patterns: RestrictedPatternMatch list) =
    let findIds (subjectMap: IriMapping) =
        match subjectMap.Value with
        | IriColumn col -> col.Name |> Set.singleton
        | IriConstant _ -> Set.empty
        | IriTemplate template ->
            (Set.empty, template)
            ||> List.fold (
                fun cur part ->
                    match part with
                    | MappingTemplate.TemplatePart.ColumnPart c -> cur |> Set.add c.Name
                    | _ -> cur
            )

    let findSource (sqlSources: Map<Pattern, SqlSource list>) idColumns pattern source =
        match source with
        | Table schema ->
            let mayBeExistingSource =
                match sqlSources.TryGetValue pattern with
                | true, variableSources ->
                    variableSources
                    |> List.tryFind (fun x -> x.Schema.Name = schema.Name)
                | false, _ ->
                    None

            let mayBeUsableSource =
                match mayBeExistingSource with
                | Some existingSource ->
                    existingSource.Schema.Keys
                    |> Seq.map (Set.ofSeq)
                    |> Seq.tryFind (
                        fun keys ->
                            keys
                            |> Set.ofSeq
                            |> Set.isSubset <| idColumns
                    )
                    |> Option.map (fun _ -> existingSource)
                | None ->
                    None
                
            mayBeUsableSource
            |> Option.map (
                fun x -> x, sqlSources
            )
            |> Option.defaultWith (fun () ->
                let newSource =
                    {
                        Schema = schema
                        Columns =
                            schema.Columns
                            |> Seq.toList
                            |> List.map (fun col -> { Schema = schema.GetColumn(col) })
                    }
                let updatedSources = newSource :: (sqlSources |> Map.tryFind pattern |> Option.defaultValue List.empty)
                newSource, sqlSources |> Map.add pattern updatedSources
            )

        | Statement _ -> "SQL Statements are not yet supported" |> NotImplementedException |> raise

    let applyPatternMatch filters (valueBindings: Map<_,_>) source pattern mapping =
        let variables =
            let neededVariables =
                match mapping with
                | IriObject iriObj ->
                    match iriObj.Value with
                    | IriColumn col -> col.Name |> List.singleton
                    | IriConstant _ -> List.empty
                    | IriTemplate tmpl ->
                        tmpl
                        |> List.choose (
                            function
                            | MappingTemplate.ColumnPart col -> col.Name |> Some
                            | _ -> None
                        )
                | LiteralObject litObj ->
                    match litObj.Value with
                    | LiteralColumn col -> col.Name |> List.singleton
                    | LiteralConstant _ -> List.empty
                    | LiteralTemplate tmpl ->
                        tmpl 
                        |> List.choose (
                            function
                            | MappingTemplate.ColumnPart col -> col.Name |> Some
                            | _ -> None
                        )

            neededVariables
            |> List.map (
                fun var ->
                    var, source |> SqlSource.getColumn var |> Column
            )
            |> Map.ofList

        let valueBinder = BaseValueBinder(mapping, variables)

        match pattern with
        | VariablePattern var ->
            match valueBindings.TryGetValue var with
            | true, otherValueBinder ->
                isBaseValueBinderBoundCondition variables :: valueBindersEqualValueCondition typeIndexer valueBinder otherValueBinder :: filters, valueBindings
            | false, _ ->
                isBaseValueBinderBoundCondition variables :: filters, valueBindings |> Map.add var valueBinder
        | NodeMatchPattern node ->
            isBaseValueBinderBoundCondition variables :: valueBinderValueEqualToNodeCondition typeIndexer valueBinder node :: filters, valueBindings

    let rec implPatternList sqlSources filters valueBindings toProcess =
        match toProcess with
        | [] ->
            {
                Model = {
                    Sources =
                        sqlSources
                        |> Map.toList
                        |> List.collect snd
                        |> List.map Sql
                    Assignments = []
                    Filters = filters |> List.distinct
                } |> NotModified |> optimizeCalculusModel
                Bindings = valueBindings
                Variables = valueBindings |> Map.keys |> List.ofSeq
            }
        | current :: xs ->
            implPatternSubject sqlSources filters valueBindings current xs

    and implPatternSubject sqlSources filters valueBindings current toProcess =
        let idColumns = findIds current.Restriction.Subject.Value
        let (source, newSqlSources) = findSource sqlSources idColumns current.PatternMatch.Subject current.Restriction.TriplesMap.Source
        let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings source current.PatternMatch.Subject (current.Restriction.Subject.Value |> IriObject)
        implPatternPredicate newSqlSources newFilters newValueBindings source current toProcess

    and implPatternPredicate sqlSources filters valueBindings source current toProcess =
        let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings source current.PatternMatch.Predicate (current.Restriction.Predicate |> IriObject)
        implPatternObject sqlSources newFilters newValueBindings source current toProcess

    and implPatternObject sqlSources filters valueBindings source current toProcess =
        match current.Restriction.Object with
        | ObjectMatch objMatch ->
            let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings source current.PatternMatch.Object objMatch
            implPatternList sqlSources newFilters newValueBindings toProcess

        | RefObjectMatch refObjMatch ->
            let refSubject = refObjMatch.TargetSubjectMap
            let idColumns = findIds refSubject.Value
            let (refSource, newSqlSources) = findSource sqlSources idColumns current.PatternMatch.Object refSubject.TriplesMap.Source
            let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings refSource current.PatternMatch.Object (refSubject.Value |> IriObject)
            let joinCondition =
                refObjMatch.JoinConditions
                |> List.map (
                    fun joinCondition ->
                        let childVariable = source |> SqlSource.getColumn joinCondition.ChildColumn |> Column
                        let targetVariable = refSource |> SqlSource.getColumn joinCondition.TargetColumn |> Column

                        EqualVariables(childVariable, targetVariable)
                        |> optimizeRelationalCondition
                )
                |> Conjunction
                |> optimizeRelationalCondition

            implPatternList newSqlSources (joinCondition :: newFilters) newValueBindings toProcess

    implPatternList Map.empty [] Map.empty patterns

let private processJoin (typeIndexer: TypeIndexer) (left: BoundCalculusModel) (right: BoundCalculusModel) =
    let leftValueBinders = left.Bindings
    let rightValueBinders = right.Bindings
    let leftVariables = leftValueBinders |> Map.toSeq |> Seq.map fst |> Set.ofSeq
    let rightVariables = rightValueBinders |> Map.toSeq |> Seq.map fst |> Set.ofSeq
    let sharedVariables =
        Set.intersect leftVariables rightVariables
    let onlyLeftVariables = Set.difference leftVariables sharedVariables
    let onlyRightVariables = Set.difference rightVariables sharedVariables

    let joinConditions =
        sharedVariables
        |> Set.toList
        |> List.map (
            fun variable ->
                let leftBinder = leftValueBinders.[variable] |> valueBinderToExpressionSet typeIndexer
                let rightBinder = rightValueBinders.[variable] |> valueBinderToExpressionSet typeIndexer

                [
                    leftBinder.IsNotErrorCondition |> Not |> optimizeRelationalCondition
                    rightBinder.IsNotErrorCondition |> Not |> optimizeRelationalCondition
                    expressionSetValuesEqualCondition leftBinder rightBinder
                ]
                |> Disjunction
                |> optimizeRelationalCondition
        )

    let oneSideValueBinders variables binders =
        binders
        |> Map.filter (fun v _ -> variables |> Set.contains v)

    let sharedValueBinders =
        sharedVariables
        |> Set.toSeq
        |> Seq.map (
            fun v ->
                v,
                [
                    leftValueBinders.[v]
                    rightValueBinders.[v]
                ] |> CoalesceValueBinder
        )
        |> Map.ofSeq

    let valueBinders =
        oneSideValueBinders onlyLeftVariables leftValueBinders
        |> Map.union (oneSideValueBinders onlyRightVariables rightValueBinders)
        |> Map.union sharedValueBinders

    valueBinders, joinConditions

let rec private processSparqlPattern (database: ISqlDatabaseSchema) (typeIndexer: TypeIndexer) (sparqlPattern: SparqlPattern) =
    let getNotModifiedModel model =
        match model with
        | NotModified notModified ->
            notModified
        | _ ->
            { Sources = SubQuery model |> List.singleton; Assignments = List.empty; Filters = List.empty }

    let rec applyOnNotModifiedModel applyFunction model =
        model
        |> getNotModifiedModel
        |> applyFunction
        |> NotModified
        |> optimizeCalculusModel

    optimizeBoundCalculusModel <|
    match sparqlPattern with
    | EmptyPattern -> 
        {
            Model = SingleEmptyResult
            Bindings = Map.empty
            Variables = List.empty
        }

    | NotMatchingPattern ->
        {
            Model = NoResult
            Bindings = Map.empty
            Variables = List.empty
        }

    | NotProcessedTriplePatterns _ ->
        "Encountered NotProcessedTriplePatterns in RelationalAlgebraBuilder"
        |> invalidArg "sparqlPattern"

    | RestrictedTriplePatterns restrictedPatterns ->
        restrictedPatterns
        |> processRestrictedTriplePattern typeIndexer

    | FilterPattern(inner, condition) ->
        let processedInner = inner |> processSparqlPattern database typeIndexer
        let (conditionNonError, conditionTrue) = condition |> processSparqlCondition typeIndexer processedInner.Bindings
        { processedInner with
            Model =
                processedInner.Model
                |> applyOnNotModifiedModel (
                    fun innerModel ->
                        { innerModel with
                            Filters = conditionNonError :: conditionTrue :: innerModel.Filters
                        }
                )
        }

    | ExtendPattern(inner, extensions) ->
        let processedInner = inner |> processSparqlPattern database typeIndexer

        { processedInner with
            Bindings =
                (processedInner.Bindings, extensions)
                ||> List.fold (
                    fun bindings (variable, expression) ->
                        match bindings.TryGetValue variable with
                        | true, _ ->
                            sprintf "Tried to extend graph with a variable that is already present: %A" variable
                            |> invalidOp
                        | false, _ ->
                            let expressionSet = processSparqlExpression typeIndexer bindings expression
                            bindings |> Map.add variable (expressionSet |> ExpressionValueBinder)
                )
        }

    | JoinPattern inners ->
        inners
        |> List.map (processSparqlPattern database typeIndexer)
        |> List.reduce (
            fun left right ->
                let (valueBinders, joinConditions) = processJoin typeIndexer left right

                let leftModel = left.Model |> getNotModifiedModel
                let rightModel = right.Model |> getNotModifiedModel

                {
                    Model =
                        {
                            Sources = leftModel.Sources @ rightModel.Sources
                            Assignments = leftModel.Assignments @ rightModel.Assignments
                            Filters = leftModel.Filters @ rightModel.Filters @ joinConditions
                        }
                        |> NotModified
                        |> optimizeCalculusModel
                    Bindings = valueBinders
                    Variables = valueBinders |> Map.keys |> List.ofSeq
                }
        )

    | LeftJoinPattern(left, right, condition) ->
        let leftProc = left |> processSparqlPattern database typeIndexer
        let rightProc = right |> processSparqlPattern database typeIndexer
        let (valueBinders, joinConditions) = processJoin typeIndexer leftProc rightProc
        let conditionProc =
            condition
            |> processSparqlCondition typeIndexer valueBinders
            |> fun (x, y) -> [x; y]
            |> Conjunction
            |> optimizeRelationalCondition

        let leftModel = leftProc.Model |> getNotModifiedModel
        let rightModel = rightProc.Model |> getNotModifiedModel

        {
            Model =
                { leftModel with
                    Sources =
                        LeftOuterJoinModel(
                            rightModel,
                            conditionProc :: joinConditions
                            |> Conjunction
                            |> optimizeRelationalCondition
                        ) :: leftModel.Sources
                }
                |> NotModified
                |> optimizeCalculusModel
            Bindings = valueBinders
            Variables = valueBinders |> Map.keys |> List.ofSeq
        }

    | UnionPattern unioned ->
        let switchVariable = { SqlType = database.IntegerType }
        let (models, valueBindersCases) =
            unioned
            |> List.map (processSparqlPattern database typeIndexer)
            |> List.mapi (
                fun i proc ->
                    let procModel = proc.Model |> getNotModifiedModel
                    { procModel with
                        Assignments = { Variable = switchVariable; Expression = i |> Int |> Constant } :: procModel.Assignments
                    }, proc.Bindings |> Map.toList |> List.map (fun (v, vb) -> v, (i, vb))
            )
            |> List.unzip

        let valueBindings =
            valueBindersCases
            |> List.concat
            |> List.groupBy fst
            |> List.map (
                fun (variable, binderCases) ->
                    let vb =
                        binderCases
                        |> List.map snd
                        |> Map.ofList
                        |> fun x -> CaseValueBinder(switchVariable |> Assigned, x)
                    variable, vb
            )
            |> Map.ofList

        {
            Model = (switchVariable, models) |> Union |> optimizeCalculusModel
            Bindings = valueBindings
            Variables = valueBindings |> Map.keys |> List.ofSeq
        }

let private applyModifiers (typeIndexer: TypeIndexer) (modifiers: Modifier list) (inner: BoundCalculusModel) =
    (inner, modifiers)
    ||> List.fold (
        fun procInner modifier ->
            let rec updateModified apply model =
                match model with
                | NoResult ->
                    NoResult

                | Modified m ->
                    m
                    |> apply
                    |> optimizeCalculusModel

                | NotModified nm ->
                    {
                        InnerModel = nm |> NotModified
                        Ordering = List.empty
                        Limit = None
                        Offset = None
                        IsDistinct = false
                    }
                    |> Modified
                    |> updateModified apply

                | _ ->
                    { Sources = SubQuery model |> List.singleton; Assignments = List.empty; Filters = List.empty }
                    |> NotModified
                    |> updateModified apply

            optimizeBoundCalculusModel <|
            match modifier with
            | Select variables ->
                { procInner with 
                    Bindings =
                        variables
                        |> List.map (
                            fun v ->
                                match procInner.Bindings.TryGetValue v with
                                | true, vb -> v, vb
                                | false, _ -> v, EmptyValueBinder
                        )
                        |> Map.ofList
                    Variables = variables
                }

            | Distinct ->
                ((procInner.Model, Map.empty), procInner.Bindings)
                ||> Map.fold (
                    fun (current, prevBinders) var valueBinder ->
                        match valueBinder with
                        | BaseValueBinder _ ->
                            current, prevBinders |> Map.add var valueBinder
                        | _ ->
                            sprintf "%A not yet supported for DISTINCT alignment" valueBinder
                            |> NotImplementedException
                            |> raise
                )
                |> fun (innerModel, bindings) ->
                    {
                        Model =
                            innerModel
                            |> updateModified (fun x -> { x with IsDistinct = true } |> Modified)
                        Bindings = bindings
                        Variables = procInner.Variables
                    }

            | OrderBy orderingParts ->
                (orderingParts, procInner)
                ||> List.foldBack (
                    fun orderingPart current ->
                        let vb =
                            match current.Bindings.TryGetValue orderingPart.Variable with
                            | true, x -> x
                            | false, _ -> EmptyValueBinder
                        let es =
                            vb |> valueBinderToExpressionSet typeIndexer

                        let newOrdering =
                            [
                                es.IsNotErrorCondition |> Boolean
                                es.TypeCategoryExpression
                                es.BooleanExpression
                                es.NumericExpression
                                es.DateTimeExpresion
                                es.StringExpression
                            ]
                            |> List.map (
                                fun expr ->
                                    { Expression = expr; Direction = orderingPart.Direction }
                            )

                        { procInner with
                            Model =
                                procInner.Model
                                |> updateModified (
                                    fun x ->
                                        { x with Ordering = newOrdering @ x.Ordering }
                                        |> Modified
                                )
                        }
                )

            | Slice slice ->
                { procInner with 
                    Model =
                        procInner.Model
                        |> updateModified (
                            fun x ->
                                { x with
                                    Limit =
                                        slice.Limit
                                        |> Option.bind (
                                            fun limit ->
                                                x.Limit
                                                |> Option.map (
                                                    fun innerLimit ->
                                                        Math.Min(limit, innerLimit - (slice.Offset |> Option.defaultValue 0))
                                                )
                                                |> Option.orElse slice.Limit
                                        )
                                        |> Option.orElse x.Limit
                                    Offset =
                                        slice.Offset
                                        |> Option.bind (
                                            fun offset ->
                                                x.Offset
                                                |> Option.map (fun innerOffset -> offset + innerOffset)
                                                |> Option.orElse slice.Offset
                                        )
                                        |> Option.orElse x.Offset
                                }
                                |> Modified
                        )
                }
    )

let buildRelationalQuery (database: ISqlDatabaseSchema) (typeIndexer: TypeIndexer) (sparqlAlgebra: SparqlQuery) =
    sparqlAlgebra.Query
    |> processSparqlPattern database typeIndexer
    |> applyModifiers typeIndexer sparqlAlgebra.Modifiers
