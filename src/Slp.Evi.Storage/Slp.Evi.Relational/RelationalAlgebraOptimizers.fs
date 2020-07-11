module Slp.Evi.Relational.RelationalAlgebraOptimizers

open Slp.Evi.Common.Algebra
open Slp.Evi.Relational.Algebra
open Slp.Evi.Relational.RelationalAlgebraNormalizer

let optimizeRelationalExpression expression =
    expression
    |> normalizeRelationalExpression

let rec optimizeRelationalCondition condition =
    let optimizeConcatenationsEquality leftConcat rightConcat =
        match ConcatenationEqualityOptimizer.compareConcatenations leftConcat rightConcat with
        | Some(processed) ->
            processed
            |> List.map (
                function
                | ConcatenationEqualityOptimizer.AlwaysMatching -> AlwaysTrue
                | ConcatenationEqualityOptimizer.AlwaysNotMatching -> AlwaysFalse
                | ConcatenationEqualityOptimizer.MatchingCondition(left, right) ->
                    if left = leftConcat && right = rightConcat then
                        Comparison(Comparisons.EqualTo, left |> Concatenation, right |> Concatenation)
                    else
                        Comparison(
                            Comparisons.EqualTo, 
                            left |> Concatenation |> optimizeRelationalExpression,
                            right |> Concatenation |> optimizeRelationalExpression
                        )
                        |> optimizeRelationalCondition
            )
            |> Conjunction
            |> optimizeRelationalCondition
        | None ->
            Comparison(Comparisons.EqualTo, leftConcat |> Concatenation, rightConcat |> Concatenation)

    match condition |> normalizeRelationalCondition with
    | Comparison(Comparisons.EqualTo, Concatenation(left), Concatenation(right)) ->
        optimizeConcatenationsEquality left right
    | Comparison(Comparisons.EqualTo, Concatenation(left), (Constant(_) as right)) ->
        optimizeConcatenationsEquality left [ right ]
    | Comparison(Comparisons.EqualTo, (Constant(_) as left), Concatenation(right)) ->
        optimizeConcatenationsEquality [ left ] right
    | Comparison(Comparisons.EqualTo, IriSafeVariable(left), IriSafeVariable(right)) ->
        Comparison(Comparisons.EqualTo, Variable left, Variable right) |> optimizeRelationalCondition
    | Comparison(Comparisons.EqualTo, Constant(constX), Constant(constY)) ->
        match constX, constY with
        | Int x, Int y ->
            if x = y then AlwaysTrue else AlwaysFalse
        | String x, String y ->
            if x = y then AlwaysTrue else AlwaysFalse
        | Double x, Double y ->
            if x = y then AlwaysTrue else AlwaysFalse
        | _ ->
            condition
    | IsNull(Column(col)) when col.Schema.SqlType.IsNullable |> not ->
        AlwaysFalse
    | EqualVariables(x, y) when x = y ->
        AlwaysTrue
    | _ ->
        condition
    |> normalizeRelationalCondition

let optimizeCalculusModel model =
    model
    |> normalizeCalculusModel

let optimizeBoundCalculusModel model =
    model
    |> normalizeBoundCalculusModel