module Slp.Evi.Relational.RelationalAlgebraOptimizers

open Slp.Evi.Relational.RelationalAlgebraNormalizer

let optimizeRelationalExpression expression =
    expression
    |> normalizeRelationalExpression

let optimizeRelationalCondition condition =
    condition
    |> normalizeRelationalCondition

let optimizeCalculusModel model =
    model
    |> normalizeCalculusModel

let optimizeBoundCalculusModel model =
    model
    |> normalizeBoundCalculusModel