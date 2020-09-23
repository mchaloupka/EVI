module Slp.Evi.Storage.Core.ValueBinderLoader

open VDS.RDF
open Slp.Evi.Common.Algebra
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Database
open Slp.Evi.Relational.Algebra
open TCode.r2rml4net

let getVariableName =
    function
    | SparqlVariable x -> x
    | BlankNodeVariable x -> x

let rec loadValueFromExpression (namingProvider: NamingProvider) (row: ISqlResultRow) (expression: Expression) =
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
        match namingProvider.TryGetVariableName var with
        | false, _ ->
            sprintf "Cannot find name for a variable: %A" var
            |> invalidOp
        | true, variableName ->
            variableName
            |> row.GetColumn
            |> fun x -> x.VariableValue

    | IriSafeVariable var ->
        let variableValue = var |> Variable |> loadValueFromExpression namingProvider row
        variableValue |> VariableValue.asString |> MappingHelper.UrlEncode |> StringVariableValue
    | Constant c ->
        match c with
        | String s -> StringVariableValue s
        | Int i -> IntVariableValue i
        | Double d -> DoubleVariableValue d

    | Concatenation expressions ->
        let sb = new System.Text.StringBuilder()

        expressions
        |> List.iter (loadValueFromExpression namingProvider row >> sb.Append >> ignore)

        sb.ToString () |> StringVariableValue

    | Boolean condition ->
        condition |> loadValueFromCondition namingProvider row

    | Null ->
        NullVariableValue

and loadValueFromCondition (namingProvider: NamingProvider) (row: ISqlResultRow) (condition: Condition) =
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
            | StringVariableValue _, _
            | _, StringVariableValue _ ->
                sprintf "Cannot process the comparison as only one of the values is textual: %A" condition
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

let rec loadValue (rdfHandler: INodeFactory) (namingProvider: NamingProvider) (row: ISqlResultRow) (valueBinder: ValueBinder): INode option =
    match valueBinder with
    | EmptyValueBinder ->
        None

    | CoalesceValueBinder valueBinders ->
        let rec getFirstDefined previous next =
            match previous, next with
            | Some(_), _ -> previous
            | None, [] -> None
            | None, x :: xs ->
                x |> loadValue rdfHandler namingProvider row |> getFirstDefined <| xs
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
            loadValue rdfHandler namingProvider row vb

    | ExpressionValueBinder expressionSet ->
        new System.NotImplementedException () |> raise

    | BaseValueBinder (objectMapping, baseValueBinder) ->
        new System.NotImplementedException () |> raise