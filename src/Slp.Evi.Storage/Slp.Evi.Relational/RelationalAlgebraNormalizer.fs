module Slp.Evi.Relational.RelationalAlgebraNormalizer

open Slp.Evi.Common.Algebra
open Slp.Evi.Relational.Algebra

let normalizeRelationalExpression expression =
    match expression with
    | Null
    | Constant _
    | Variable _
    | IriSafeVariable _
    | Boolean _
    | BinaryNumericOperation _ ->
        expression

    | Coalesce subExpressions ->
        (subExpressions, List.empty)
        ||> List.foldBack (
            fun current restExpressions ->
                match current with
                | Constant _ -> current |> List.singleton
                | Null -> restExpressions
                | _ -> current :: restExpressions
        )
        |> function
        | [] -> Null
        | x :: [] -> x
        | xs -> xs |> Coalesce

    | Switch caseStatements ->
        (caseStatements, List.empty)
        ||> List.foldBack (
            fun current restStatements ->
                match current with
                | { Condition = AlwaysTrue; Expression = _ } ->
                    current |> List.singleton
                | { Condition = AlwaysFalse; Expression = _ } ->
                    restStatements
                | _ -> current :: restStatements
        )
        |> function
        | [] -> Null
        | { Condition = AlwaysTrue; Expression = expr } :: _ -> expr
        | xs -> xs |> Switch

    | Concatenation parts ->
        (parts, List.empty)
        ||> List.foldBack (
            fun cur next ->
                match cur with
                | Constant(String "") -> next
                | _ -> cur :: next
        )
        |> function
        | [] -> String "" |> Constant
        | x :: [] -> x
        | xs -> xs |> Concatenation

let normalizeRelationalCondition condition =
    match condition with
    | Conjunction conditions ->
        (conditions, List.empty)
        ||> List.foldBack (
            fun current rest ->
                match current with
                | AlwaysTrue -> rest
                | AlwaysFalse -> AlwaysFalse |> List.singleton
                | Conjunction(inner) -> inner @ rest
                | _ -> current :: rest
        )
        |> function
        | [] -> AlwaysTrue
        | AlwaysFalse :: _ -> AlwaysFalse
        | x :: [] -> x
        | xs -> Conjunction(xs)
    | Disjunction conditions ->
        (conditions, List.empty)
        ||> List.foldBack (
            fun current rest ->
                match current with
                | AlwaysFalse -> rest
                | AlwaysTrue -> AlwaysTrue |> List.singleton
                | Disjunction(inner) -> inner @ rest
                | _ -> current :: rest
        )
        |> function
        | [] -> AlwaysFalse
        | AlwaysTrue :: _ -> AlwaysTrue
        | x :: [] -> x
        | xs -> Disjunction(xs)
    | Not AlwaysTrue ->
        AlwaysFalse
    | Not AlwaysFalse ->
        AlwaysTrue
    | Not(Not(x)) ->
        x
    | Comparison(EqualTo, Variable x, Variable y) ->
        EqualVariables(x, y)
    | Comparison(EqualTo, Variable x, Constant y)
    | Comparison(EqualTo, Constant y, Variable x) ->
        EqualVariableTo(x, y)
    | AlwaysFalse
    | AlwaysTrue
    | Comparison _
    | EqualVariables _
    | EqualVariableTo _
    | IsNull _
    | LanguageMatch _
    | Like _
    | Not _ ->
        condition
    

let normalizeCalculusModel model =
    match model with
    | NotModified notModified ->
        notModified.Filters
        |> Conjunction
        |> normalizeRelationalCondition
        |> function
        | AlwaysFalse ->
            NoResult
        | Conjunction(filters) ->
            { notModified with Filters = filters } |> NotModified
        | x ->
            { notModified with Filters = x |> List.singleton } |> NotModified
    | Modified _ ->
        model
    | NoResult -> NoResult
    | SingleEmptyResult -> SingleEmptyResult

let normalizeBoundCalculusModel model =
    model