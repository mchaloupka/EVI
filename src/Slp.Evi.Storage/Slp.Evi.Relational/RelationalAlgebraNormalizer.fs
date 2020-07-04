module Slp.Evi.Relational.RelationalAlgebraNormalizer

open Slp.Evi.Relational.Algebra

let normalizeRelationalExpression expression =
    match expression with
    | Null
    | Constant _
    | Variable _ 
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

let normalizeRelationalCondition condition =
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

    | Modified modified ->
        model
    | NoResult -> NoResult
    | SingleEmptyResult -> SingleEmptyResult

let normalizeModifiedCalculus model =
    model

let normalizeBoundCalculusModel model =
    model