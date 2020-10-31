module Slp.Evi.Storage.Core.Relational.RelationalAlgebraNormalizer

open Slp.Evi.Storage.Core.Common.Algebra
open Slp.Evi.Storage.Core.Relational.Algebra

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
                match current, rest with
                | _, [ AlwaysFalse ] -> rest
                | AlwaysTrue, _ -> rest
                | AlwaysFalse, _ -> AlwaysFalse |> List.singleton
                | Conjunction(inner), _ -> inner @ rest
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
                match current, rest with
                | _, [ AlwaysTrue ] -> rest
                | AlwaysFalse, _ -> rest
                | AlwaysTrue, _ -> AlwaysTrue |> List.singleton
                | Disjunction(inner), _ -> inner @ rest
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
        let updatedFilters =
            notModified.Filters
            |> Conjunction
            |> normalizeRelationalCondition
            |> function
            | AlwaysFalse ->
                AlwaysFalse |> List.singleton
            | Conjunction(filters) ->
                filters
            | x ->
                x |> List.singleton

        (notModified.Sources, List.empty)
        ||> List.foldBack (
            fun toAdd current ->
                match toAdd, current with
                | _, [ SubQuery NoResult ] -> current
                | SubQuery SingleEmptyResult, _ -> current
                | SubQuery NoResult, _ -> toAdd |> List.singleton
                | _, _ -> toAdd :: current
        )
        |> fun x -> x, updatedFilters, notModified.Assignments
        |> function
        | [ SubQuery NoResult ], _, _
        | _, AlwaysFalse :: _, _ ->
            NoResult
        | [ SubQuery SingleEmptyResult ], [ AlwaysTrue ], [] ->
            SingleEmptyResult
        | sources, filters, assignments ->
            { Sources = sources; Filters = filters; Assignments = assignments }
            |> NotModified
    | Modified _ ->
        model
    | NoResult -> NoResult
    | SingleEmptyResult -> SingleEmptyResult
    | Union (variable, unioned) ->
        unioned
        |> List.filter (
            function
            | { Sources = [ SubQuery NoResult ]; Assignments = _; Filters = _ } ->
                false
            | _ ->
                true
        )
        |> function
        | [] -> NoResult
        | x :: [] -> x |> NotModified
        | xs -> Union(variable, xs)

let normalizeBoundCalculusModel model =
    model

let normalizeValueBinder valueBinder =
    valueBinder
