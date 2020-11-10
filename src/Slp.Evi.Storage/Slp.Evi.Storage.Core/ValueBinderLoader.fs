module Slp.Evi.Storage.Core.ValueBinderLoader

open VDS.RDF
open Slp.Evi.Storage.Core.Common
open Slp.Evi.Storage.Core.Common.Algebra
open Slp.Evi.Storage.Core.Common.Types
open Slp.Evi.Storage.Core.Common.Database
open Slp.Evi.Storage.Core.R2RML
open Slp.Evi.Storage.Core.Sparql.Algebra
open Slp.Evi.Storage.Core.Database
open Slp.Evi.Storage.Core.Relational
open Slp.Evi.Storage.Core.Relational.Algebra
open TCode.r2rml4net

let getVariableName =
    function
    | SparqlVariable x -> x
    | BlankNodeVariable x -> x

let private loadValueFromVariable (namingProvider: NamingProvider) (row: ISqlResultRow) (variable: Variable) =
    match namingProvider.TryGetVariableName variable with
    | false, _ ->
        sprintf "Cannot find name for a variable: %A" variable
        |> invalidOp
    | true, variableName ->
        variableName
        |> row.GetColumn
        |> fun x -> x.VariableValue

let rec private loadValueFromExpression (namingProvider: NamingProvider) (row: ISqlResultRow) (expression: Expression) =
    match expression with
    | BinaryNumericOperation (operator, leftExpression, rightExpression) ->
        let leftValue = leftExpression |> loadValueFromExpression namingProvider row
        let rightValue = rightExpression |> loadValueFromExpression namingProvider row

        let rec evaluateArithmeticExpression left right =
            match left, right with
            | IntVariableValue l, IntVariableValue r ->
                match operator with
                | Add -> l + r |> IntVariableValue
                | Subtract -> l - r |> IntVariableValue
                | Multiply -> l * r |> IntVariableValue
                | Divide -> l / r |> IntVariableValue

            | DoubleVariableValue l, DoubleVariableValue r ->
                match operator with
                | Add -> l + r |> DoubleVariableValue
                | Subtract -> l - r |> DoubleVariableValue
                | Multiply -> l * r |> DoubleVariableValue
                | Divide -> l / r |> DoubleVariableValue

            | (DoubleVariableValue _) as d, IntVariableValue i ->
                i |> double |> DoubleVariableValue
                |> evaluateArithmeticExpression d

            | IntVariableValue i, ((DoubleVariableValue _) as d) ->
                i |> double |> DoubleVariableValue
                |> evaluateArithmeticExpression <| d

            | BooleanVariableValue b, o ->
                if b then 1 else 0
                |> IntVariableValue
                |> evaluateArithmeticExpression <| o

            | o, BooleanVariableValue b ->
                if b then 1 else 0
                |> IntVariableValue
                |> evaluateArithmeticExpression o

            | StringVariableValue _, _
            | _, StringVariableValue _ ->
                sprintf "Cannot process the arithmetic expression as one of the values is textual: %A" expression
                |> invalidOp

            | DateTimeVariableValue _, _
            | _, DateTimeVariableValue _ ->
                sprintf "Cannot process the arithmetic expression as one of the values is date time: %A" expression
                |> invalidOp

            | NullVariableValue, _
            | _, NullVariableValue ->
                NullVariableValue

        evaluateArithmeticExpression leftValue rightValue

    | Switch caseStatements ->
        let rec evaluateCaseStatements = function
            | [] ->
                NullVariableValue
            | x :: xs ->
                let evaluatedCaseCondition: VariableValue =
                    x.Condition
                    |> loadValueFromCondition namingProvider row

                if evaluatedCaseCondition |> VariableValue.asBoolean then
                    x.Expression |> loadValueFromExpression namingProvider row
                else
                    xs |> evaluateCaseStatements

        caseStatements |> evaluateCaseStatements

    | Coalesce coalesced ->
        let rec getFirstDefined previous next =
            match previous, next with
            | NullVariableValue, [] ->
                NullVariableValue
            | NullVariableValue, x :: xs ->
                x |> loadValueFromExpression namingProvider row |> getFirstDefined <| xs
            | x, _ ->
                x

        getFirstDefined NullVariableValue coalesced

    | Variable var ->
        loadValueFromVariable namingProvider row var

    | IriSafeVariable var ->
        let variableValue = var |> Variable |> loadValueFromExpression namingProvider row
        variableValue |> VariableValue.asString |> MappingHelper.UrlEncode |> StringVariableValue
    | Constant c ->
        match c with
        | String s -> StringVariableValue s
        | Int i -> IntVariableValue i
        | Double d -> DoubleVariableValue d
        | DateTimeLiteral d -> DateTimeVariableValue d

    | Concatenation expressions ->
        let sb = new System.Text.StringBuilder()

        expressions
        |> List.iter (loadValueFromExpression namingProvider row >> sb.Append >> ignore)

        sb.ToString () |> StringVariableValue

    | Boolean condition ->
        condition |> loadValueFromCondition namingProvider row

    | Null ->
        NullVariableValue

and private loadValueFromCondition (namingProvider: NamingProvider) (row: ISqlResultRow) (condition: Condition) =
    BooleanVariableValue <|
    match condition with
    | AlwaysFalse ->
         false
    | AlwaysTrue ->
         true
    | Conjunction conditions ->
        conditions |> List.forall (loadValueFromCondition namingProvider row >> VariableValue.asBoolean)
    | Disjunction conditions ->
        conditions |> List.exists (loadValueFromCondition namingProvider row >> VariableValue.asBoolean)
    | EqualVariableTo (variable, literal) ->
        let variableValue = variable |> Variable |> loadValueFromExpression namingProvider row
        let literalValue = literal |> Constant |> loadValueFromExpression namingProvider row
        variableValue = literalValue
    | EqualVariables (lv, rv) ->
        let lValue = lv |> Variable |> loadValueFromExpression namingProvider row
        let rValue = rv |> Variable |> loadValueFromExpression namingProvider row
        lValue = rValue
    | IsNull variable ->
        let value = variable |> Variable |> loadValueFromExpression namingProvider row
        match value with
        | NullVariableValue -> true
        | _ -> false
    | Not condition ->
        condition
        |> loadValueFromCondition namingProvider row
        |> VariableValue.asBoolean
        |> not
    | Comparison(comparison, leftExpression, rightExpression) ->
        let rec evaluateComparison leftValue rightValue =
            match leftValue, rightValue with
            | IntVariableValue l, IntVariableValue r ->
                match comparison with
                | GreaterThan -> l > r
                | GreaterOrEqualThan -> l >= r
                | LessThan -> l < r
                | LessOrEqualThan -> l <= r
                | EqualTo -> l = r
            | DoubleVariableValue l, DoubleVariableValue r ->
                match comparison with
                | GreaterThan -> l > r
                | GreaterOrEqualThan -> l >= r
                | LessThan -> l < r
                | LessOrEqualThan -> l <= r
                | EqualTo -> l = r
            | StringVariableValue l, StringVariableValue r ->
                match comparison with
                | GreaterThan -> l > r
                | GreaterOrEqualThan -> l >= r
                | LessThan -> l < r
                | LessOrEqualThan -> l <= r
                | EqualTo -> l = r
            | DateTimeVariableValue l, DateTimeVariableValue r ->
                match comparison with
                | GreaterThan -> l > r
                | GreaterOrEqualThan -> l >= r
                | LessThan -> l < r
                | LessOrEqualThan -> l <= r
                | EqualTo -> l = r
            | DateTimeVariableValue _, _
            | _, DateTimeVariableValue _ ->
                sprintf "Cannot process the comparison as exactly one of the values is date time: %A" condition
                |> invalidOp
            | StringVariableValue _, _
            | _, StringVariableValue _ ->
                sprintf "Cannot process the comparison as exactly one of the values is string: %A" condition
                |> invalidOp
            | (DoubleVariableValue _) as d, IntVariableValue i ->
                i |> double |> DoubleVariableValue
                |> evaluateComparison d
            | IntVariableValue i, ((DoubleVariableValue _) as d) ->
                i |> double |> DoubleVariableValue
                |> evaluateComparison <| d
            | BooleanVariableValue b, o ->
                if b then 1 else 0
                |> IntVariableValue
                |> evaluateComparison <| o
            | o, BooleanVariableValue b ->
                if b then 1 else 0
                |> IntVariableValue
                |> evaluateComparison o
            | NullVariableValue, _
            | _, NullVariableValue ->
                false

        (leftExpression |> loadValueFromExpression namingProvider row, rightExpression |> loadValueFromExpression namingProvider row)
        ||> evaluateComparison

    | Like _ ->
        new System.NotImplementedException() |> raise
    | LanguageMatch _ ->
        new System.NotImplementedException() |> raise

type BlankNodeCache = System.Collections.Generic.Dictionary<string, IBlankNode>

module BlankNodeCache =
    let create (): BlankNodeCache =
        new System.Collections.Generic.Dictionary<string, IBlankNode>()

    let getBlankNode (factory: INodeFactory) value (cache: BlankNodeCache) =
        match cache.TryGetValue value with
        | true, node ->
            node
        | false, _ ->
            let node = factory.CreateBlankNode()
            cache.Add(value, node)
            node

let rec loadValue (rdfHandler: INodeFactory) (blankNodeCache: BlankNodeCache) (typeIndexer: TypeIndexer) (namingProvider: NamingProvider) (row: ISqlResultRow) (valueBinder: ValueBinder): INode option =
    match valueBinder with
    | EmptyValueBinder ->
        None

    | CoalesceValueBinder valueBinders ->
        let rec getFirstDefined previous next =
            match previous, next with
            | Some(_), _ -> previous
            | None, [] -> None
            | None, x :: xs ->
                x |> loadValue rdfHandler blankNodeCache typeIndexer namingProvider row |> getFirstDefined <| xs
        getFirstDefined None valueBinders

    | CaseValueBinder(caseVariable, casesMap) ->
        let caseVariableValue =
            caseVariable
            |> Variable
            |> loadValueFromExpression namingProvider row

        match caseVariableValue |> VariableValue.asInteger |> casesMap.TryGetValue with
        | false, _ ->
            None
        | true, vb ->
            loadValue rdfHandler blankNodeCache typeIndexer namingProvider row vb

    | ExpressionValueBinder expressionSet ->
        let isBound =
            expressionSet.IsNotErrorCondition
            |> loadValueFromCondition namingProvider row
            |> VariableValue.asBoolean

        if isBound then
            let typeIndex =
                expressionSet.TypeExpression
                |> loadValueFromExpression namingProvider row
                |> VariableValue.asInteger

            let valueType =
                typeIndex |> typeIndexer.FromIndex

            let value =
                expressionSet.StringExpression
                |> loadValueFromExpression namingProvider row
                |> VariableValue.asString

            match valueType.NodeType with
            | BlankNodeType ->
                blankNodeCache
                |> BlankNodeCache.getBlankNode rdfHandler value
                :> INode
            | IriNodeType ->
                new System.Uri(value)
                |> rdfHandler.CreateUriNode
                :> INode
            | LiteralNodeType DefaultType ->
                rdfHandler.CreateLiteralNode value
                :> INode
            | LiteralNodeType (WithType iri) ->
                rdfHandler.CreateLiteralNode(value, iri |> Iri.toUri)
                :> INode
            | LiteralNodeType (WithLanguage lang) ->
                rdfHandler.CreateLiteralNode(value, lang)
                :> INode
            |> Some

        else
            None

    | ConditionedValueBinder (condition, valueBinder) ->
        let conditionValue =
            condition
            |> loadValueFromCondition namingProvider row
            |> VariableValue.asBoolean

        if conditionValue then
            valueBinder
            |> loadValue rdfHandler blankNodeCache typeIndexer namingProvider row
        else
            None

    | BaseValueBinder (objectMapping, variableMappings) ->
        let getValueForColumn (column: SqlColumnSchema) =
            match variableMappings.TryGetValue column.Name with
            | true, variable ->
                variable
                |> loadValueFromVariable namingProvider row
            | false, _ ->
                sprintf "Base value binder references columns that is not in the mappings: %A" column
                |> invalidOp

        let loadTemplateValue (isIri: bool) (template: MappingTemplate.Template<SqlColumnSchema>) =
            let sb = new System.Text.StringBuilder ()
            let rec templateLoader parts =
                match parts with
                | [] -> sb.ToString() |> Some
                | MappingTemplate.TextPart t :: xs ->
                    sb.Append t |> ignore
                    templateLoader xs
                | MappingTemplate.ColumnPart c :: xs ->
                    let columnValue =
                        c
                        |> getValueForColumn
                        |> VariableValue.tryAsString

                    match columnValue with
                    | Some v ->
                        sb.Append v |> ignore
                        templateLoader xs
                    | None ->
                        None

            template |> templateLoader

        match objectMapping with
        | IriObject iriMapping ->
            let buildIri isBlankNode value =
                if isBlankNode then
                    blankNodeCache
                    |> BlankNodeCache.getBlankNode rdfHandler value
                    :> INode
                else
                    value
                    |> IriReference.fromString
                    |> IriReference.tryResolve None
                    |> Iri.toUri
                    |> rdfHandler.CreateUriNode
                    :> INode

            match iriMapping.Value with
            | IriColumn column ->
                column
                |> getValueForColumn
                |> VariableValue.tryAsString
                |> Option.map (buildIri iriMapping.IsBlankNode)
            | IriTemplate template ->
                template
                |> loadTemplateValue true
                |> Option.map (buildIri iriMapping.IsBlankNode)
            | IriConstant iri ->
                Some <|
                if iriMapping.IsBlankNode then
                    blankNodeCache
                    |> BlankNodeCache.getBlankNode rdfHandler (iri |> Iri.toText)
                    :> INode
                else
                    iri
                    |> Iri.toUri
                    |> rdfHandler.CreateUriNode
                    :> INode

        | LiteralObject literalMapping ->
            let buildLiteral value =
                match literalMapping.Type with
                | DefaultType ->
                    rdfHandler.CreateLiteralNode value
                    :> INode
                | WithType iri ->
                    rdfHandler.CreateLiteralNode (value, iri |> Iri.toUri)
                    :> INode
                | WithLanguage lang ->
                    rdfHandler.CreateLiteralNode (value, lang)
                    :> INode

            match literalMapping.Value with
            | LiteralColumn column ->
                column
                |> getValueForColumn
                |> VariableValue.tryAsString
                |> Option.map buildLiteral
            | LiteralTemplate template ->
                template
                |> loadTemplateValue false
                |> Option.map buildLiteral
            | LiteralConstant value ->
                value |> buildLiteral |> Some
