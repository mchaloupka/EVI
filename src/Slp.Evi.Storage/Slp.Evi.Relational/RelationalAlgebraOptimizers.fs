module Slp.Evi.Relational.RelationalAlgebraOptimizers

open Slp.Evi.Common.Algebra
open Slp.Evi.Relational.Algebra
open Slp.Evi.Relational.RelationalAlgebraNormalizer

let optimizeRelationalExpression expression =
    expression
    |> normalizeRelationalExpression

let optimizeRelationalCondition condition =
    match condition with
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
    | _ ->
        condition
    |> normalizeRelationalCondition

let optimizeCalculusModel model =
    model
    |> normalizeCalculusModel

let optimizeBoundCalculusModel model =
    model
    |> normalizeBoundCalculusModel