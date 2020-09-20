module Slp.Evi.Relational.RelationalAlgebraOptimizers

open Slp.Evi.Common.Algebra
open Slp.Evi.Relational.Algebra
open Slp.Evi.Relational.RelationalAlgebraNormalizer
open TCode.r2rml4net

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
    | Comparison(Comparisons.EqualTo, IriSafeVariable(iriSafe), Constant(c))
    | Comparison(Comparisons.EqualTo, Constant(c), IriSafeVariable(iriSafe)) ->
        match c with
        | String s ->
            if s |> Seq.forall MappingHelper.IsIUnreserved then
                EqualVariableTo(iriSafe, c)
            else
                AlwaysFalse
        | Int _
        | Double _ ->
            EqualVariableTo(iriSafe, c)
            
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
    | IsNull(Column(col)) when col.Schema.IsNullable |> not ->
        AlwaysFalse
    | EqualVariables(x, y) when x = y ->
        AlwaysTrue
    | _ ->
        condition
    |> normalizeRelationalCondition

let rec private isConstantExpression = function
    | BinaryNumericOperation (_, le, re) ->
        isConstantExpression le && isConstantExpression re
    | Switch caseStatements ->
        caseStatements
        |> List.forall (
            fun x ->
                isConstantCondition x.Condition && isConstantExpression x.Expression
        )
    | Coalesce expressions ->
        expressions
        |> List.forall isConstantExpression
    | Variable _ ->
        false
    | IriSafeVariable _ ->
        false
    | Constant _ ->
        true
    | Concatenation expressions ->
        expressions
        |> List.forall isConstantExpression
    | Boolean condition ->
        condition
        |> isConstantCondition
    | Null ->
        true

and private isConstantCondition = function
    | AlwaysFalse
    | AlwaysTrue ->
        true        
    | Comparison (_, le, re) ->
        isConstantExpression le && isConstantExpression re
    | Conjunction conditions ->
        conditions
        |> List.forall isConstantCondition
    | Disjunction conditions ->
        conditions
        |> List.forall isConstantCondition
    | EqualVariableTo _ ->
        false
    | EqualVariables _ ->
        false
    | IsNull _ ->
        false
    | LanguageMatch (le, re) ->
        isConstantExpression le && isConstantExpression re
    | Like (ex, _) ->
        isConstantExpression ex
    | Not cond ->
        isConstantCondition cond

let optimizeCalculusModel model =
    normalizeCalculusModel <|
    match model with
    | Modified modifiedCalculusModel ->
        { modifiedCalculusModel with
            Ordering =
                modifiedCalculusModel.Ordering
                |> List.filter (
                    fun x ->
                        x.Expression
                        |> isConstantExpression
                        |> not
                )
        }
        |> Modified
    | _ ->
        model
    

let optimizeBoundCalculusModel model =
    model
    |> normalizeBoundCalculusModel 