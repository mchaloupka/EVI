module Slp.Evi.Relational.RelationalAlgebraBuilder

open System

open Slp.Evi.Common
open Slp.Evi.Common.Types
open Slp.Evi.R2RML
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Relational.Algebra
open Slp.Evi.Relational.RelationalAlgebraOptimizers
open Slp.Evi.R2RML.MappingTemplate
open Slp.Evi.Common.Algebra

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

        let expression = literalNode.Value |> String |> Constant

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

let private expressionSetValuesEqualCondition left right =
    let isTypeEqual =
        Comparison(Comparisons.EqualTo, left.TypeExpression, right.TypeExpression)
        |> optimizeRelationalCondition

    let notIsValueInCategory category =
        Comparison(Comparisons.EqualTo, left.TypeCategoryExpression, category |> int |> Int |> Constant)
        |> optimizeRelationalCondition
        |> Not
        |> optimizeRelationalCondition

    let notIsValueNumeric = notIsValueInCategory TypeIndexer.TypeCategory.NumericLiteral
    let notIsValueBoolean = notIsValueInCategory TypeIndexer.TypeCategory.BooleanLiteral
    let notIsValueDateTime = notIsValueInCategory TypeIndexer.TypeCategory.DateTimeLiteral
    let notIsValueString =
        [
            notIsValueInCategory TypeIndexer.TypeCategory.BlankNode
            notIsValueInCategory TypeIndexer.TypeCategory.Iri
            notIsValueInCategory TypeIndexer.TypeCategory.StringLiteral
            notIsValueInCategory TypeIndexer.TypeCategory.SimpleLiteral
            notIsValueInCategory TypeIndexer.TypeCategory.OtherLiteral
        ]
        |> Conjunction |> optimizeRelationalCondition

    let expressionEqual selector =
        Comparison(Comparisons.EqualTo, left |> selector, right |> selector)
        |> optimizeRelationalCondition

    [
        isTypeEqual
        [ notIsValueString; expressionEqual (fun x -> x.StringExpression) ] |> Disjunction |> optimizeRelationalCondition
        [ notIsValueNumeric; expressionEqual (fun x -> x.NumericExpression) ] |> Disjunction |> optimizeRelationalCondition
        [ notIsValueBoolean; expressionEqual (fun x -> x.BooleanExpression) ] |> Disjunction |> optimizeRelationalCondition
        [ notIsValueDateTime; expressionEqual (fun x -> x.DateTimeExpresion) ] |> Disjunction |> optimizeRelationalCondition
    ]
    |> Conjunction
    |> optimizeRelationalCondition

let private valueBinderValueEqualToNodeCondition typeIndexer valueBinder node =
    (valueBinder |> valueBinderToExpressionSet typeIndexer, node |> nodeToExpressionSet typeIndexer)
    ||> expressionSetValuesEqualCondition

let private valueBindersEqualValueCondition typeIndexer valueBinder otherValueBinder =
    (valueBinder |> valueBinderToExpressionSet typeIndexer, otherValueBinder |> valueBinderToExpressionSet typeIndexer)
    ||> expressionSetValuesEqualCondition

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
                            |> Set.isSubset idColumns
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
                    Filters = filters
                } |> NotModified |> optimizeCalculusModel
                Bindings = valueBindings
                AlwaysBoundVariables = valueBindings |> Map.toList |> List.map fst
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

let private processSparqlPattern (typeIndexer: TypeIndexer) (sparqlPattern: SparqlPattern) =
    optimizeBoundCalculusModel <|
    match sparqlPattern with
    | EmptyPattern -> 
        {
            Model = SingleEmptyResult
            Bindings = Map.empty
            AlwaysBoundVariables = List.empty
        }
    | NotMatchingPattern ->
        {
            Model = NoResult
            Bindings = Map.empty
            AlwaysBoundVariables = List.empty
        }
    | NotProcessedTriplePatterns _ ->
        "Encountered NotProcessedTriplePatterns in RelationalAlgebraBuilder"
        |> invalidArg "sparqlPattern"
    | RestrictedTriplePatterns restrictedPatterns ->
        restrictedPatterns
        |> processRestrictedTriplePattern typeIndexer
    | _ ->
        sprintf "Ended with %A" sparqlPattern
        |> invalidOp

let private applyModifiers (modifiers: Modifier list) (inner: BoundCalculusModel) =
    sprintf "Ended with %A with modifiers to add %A" inner modifiers
    |> invalidOp

let buildRelationalQuery (typeIndexer: TypeIndexer) (sparqlAlgebra: SparqlQuery) =
    sparqlAlgebra.Query
    |> processSparqlPattern typeIndexer
    |> applyModifiers sparqlAlgebra.Modifiers
