module Slp.Evi.Storage.Core.Sparql.SparqlQueryNormalizer

open Algebra
open System

type private JoinDecompositionState = {
    InnerPatterns: SparqlPattern list
    TriplePatterns: BasicGraphPatternMatch list
    RestrictedTriplePatterns: RestrictedPatternMatch list
    FilterConditions: SparqlCondition list
    Extends: (SparqlVariable * SparqlExpression) list
    Unions: (SparqlPattern list) list
    IsNotMatching: Boolean
}

let private initialJoinDecompositionState =
    {
        InnerPatterns = []
        TriplePatterns = []
        RestrictedTriplePatterns = []
        FilterConditions = []
        Extends = []
        Unions = []
        IsNotMatching = false 
    }

let normalizeSparqlExpression (input: SparqlExpression) = input

let normalizeSparqlCondition = function
    | AlwaysFalseCondition ->
        AlwaysFalseCondition
    | AlwaysTrueCondition ->
        AlwaysTrueCondition
    | ComparisonCondition(_, _, _) as comparison ->
        comparison
    | ConjunctionCondition(inners) ->
        let rec collectConjunction result = function
            | [] -> result
            | AlwaysFalseCondition :: _ -> [AlwaysFalseCondition]
            | AlwaysTrueCondition :: xs -> collectConjunction result xs
            | ConjunctionCondition(subInners) :: xs -> collectConjunction result subInners |> collectConjunction <| xs
            | x :: xs -> collectConjunction (x :: result) xs

        match collectConjunction [] inners with
        | [] -> AlwaysTrueCondition
        | [x] -> x
        | _::_ as xs -> xs |> ConjunctionCondition
    | DisjunctionCondition(inners) ->
        let rec collectDisjunction result = function
            | [] -> result
            | AlwaysFalseCondition :: xs -> collectDisjunction result xs
            | AlwaysTrueCondition :: _ -> [AlwaysTrueCondition]
            | DisjunctionCondition(subInners) :: xs -> collectDisjunction result subInners |> collectDisjunction <| xs
            | x :: xs -> collectDisjunction (x :: result) xs

        match collectDisjunction [] inners with
        | [] -> AlwaysFalseCondition
        | [x] -> x
        | _::_ as xs -> xs |> DisjunctionCondition
    | IsBoundCondition(_) as isBound ->
        isBound
    | LanguageMatchesCondition(_) as languageMatch ->
        languageMatch
    | NegationCondition(NegationCondition(inner)) ->
        inner
    | NegationCondition(AlwaysTrueCondition) ->
        AlwaysFalseCondition
    | NegationCondition(AlwaysFalseCondition) ->
        AlwaysTrueCondition
    | NegationCondition(inner) ->
        NegationCondition(inner)
    | RegexCondition(_) as regex ->
        regex

// The desired structure is
// -> FILTER
// -> EXTEND
// -> JOIN
//   | LEFT JOIN (after left join, the right operand has expected order again from beginning of the chain)
//   | UNION (after union, all operands can have order again from beginning of the chain)
// -> TRIPLE PATTERNS | EMPTY PATTERN
let rec normalizeSparqlPattern (input: SparqlPattern) =
    match input with
    | EmptyPattern ->
        EmptyPattern
    | ExtendPattern(FilterPattern(inner, condition), assignments) ->
        FilterPattern(ExtendPattern(inner, assignments) |> normalizeSparqlPattern, condition)
    | ExtendPattern(NotMatchingPattern, _) -> NotMatchingPattern
    | ExtendPattern(ExtendPattern(inner, innerAssignments), outerAssignments) ->
        ExtendPattern(inner, innerAssignments @ outerAssignments)
    | ExtendPattern(_, _) -> 
        input
    | FilterPattern(FilterPattern(inner, innerCondition), outerCondition) ->
        FilterPattern(inner, [innerCondition; outerCondition] |> ConjunctionCondition |> normalizeSparqlCondition)
    | FilterPattern(NotMatchingPattern, _) -> NotMatchingPattern
    | FilterPattern(_, AlwaysFalseCondition) -> NotMatchingPattern
    | FilterPattern(inner, AlwaysTrueCondition) ->
        inner
    | FilterPattern(_, _) -> input
    | JoinPattern(joined) ->
        let decomposed =
            fun state current ->
                match current with
                | EmptyPattern ->
                    state
                | JoinPattern(joined) ->
                    { state with InnerPatterns = joined @ state.InnerPatterns }
                | NotMatchingPattern ->
                    { state with IsNotMatching = true }
                | FilterPattern(ExtendPattern(inner, assignments), condition) ->
                    { state with
                        InnerPatterns = inner :: state.InnerPatterns
                        FilterConditions = condition :: state.FilterConditions
                        Extends = assignments @ state.Extends
                    }
                | FilterPattern(inner, condition) ->
                    { state with 
                        InnerPatterns = inner :: state.InnerPatterns
                        FilterConditions = condition :: state.FilterConditions
                    }
                | ExtendPattern(inner, assignments) ->
                    { state with
                        InnerPatterns = inner :: state.InnerPatterns
                        Extends = assignments @ state.Extends
                    }
                | NotProcessedTriplePatterns(triplePatterns) ->
                    { state with TriplePatterns = triplePatterns @ state.TriplePatterns }
                | RestrictedTriplePatterns(triplePatterns) ->
                    { state with RestrictedTriplePatterns = triplePatterns @ state.RestrictedTriplePatterns }
                | UnionPattern(unioned) ->
                    { state with Unions = unioned :: state.Unions }
                | LeftJoinPattern(_, _, _) ->
                    { state with InnerPatterns = current :: state.InnerPatterns }
            |> List.fold
            <|| (initialJoinDecompositionState, joined)

        let rec compose state =
            match state with
            | { IsNotMatching = true } ->
                NotMatchingPattern
            | { RestrictedTriplePatterns = _::_ as patterns } ->
                { state with
                    InnerPatterns = RestrictedTriplePatterns(patterns) :: state.InnerPatterns
                    RestrictedTriplePatterns = []
                }
                |> compose
            | { TriplePatterns = _::_ as patterns } ->
                { state with
                    InnerPatterns = NotProcessedTriplePatterns(patterns) :: state.InnerPatterns
                    TriplePatterns = []
                }
                |> compose
            | { FilterConditions = _::_ as conditions } ->
                let inner = { state with FilterConditions = [] } |> compose |> normalizeSparqlPattern
                let condition = conditions |> ConjunctionCondition |> normalizeSparqlCondition
                FilterPattern(inner, condition)
            | { Extends = _::_ as assignments } ->
                let inner = { state with Extends=[] } |> compose |> normalizeSparqlPattern
                ExtendPattern(inner, assignments)
            | { Unions = unioned::others } ->
                unioned
                |> List.map (
                    fun current ->
                        { state with
                            InnerPatterns = current :: state.InnerPatterns
                            Unions = others
                        }
                        |> compose
                        |> normalizeSparqlPattern
                )
                |> UnionPattern
                |> normalizeSparqlPattern
            | { InnerPatterns = [x] } ->
                x
            | { InnerPatterns = _::_ as innerPatterns } ->
                innerPatterns |> JoinPattern
            | { InnerPatterns = [] } ->
                EmptyPattern

        decomposed |> compose

    | LeftJoinPattern(NotMatchingPattern, _, _) ->
        NotMatchingPattern
    | LeftJoinPattern(left, NotMatchingPattern, _)
    | LeftJoinPattern(left, EmptyPattern, _) ->
        left
    | LeftJoinPattern(FilterPattern(inner, filter), right, condition) ->
        FilterPattern(LeftJoinPattern(inner, right, condition) |> normalizeSparqlPattern, filter)
    | LeftJoinPattern(ExtendPattern(inner, assignments), right, condition) ->
        ExtendPattern(LeftJoinPattern(inner, right, condition) |> normalizeSparqlPattern, assignments)
    | LeftJoinPattern(UnionPattern(unioned), right, condition) ->
        unioned |> List.map(fun u -> LeftJoinPattern(u, right, condition) |> normalizeSparqlPattern) |> UnionPattern
    | LeftJoinPattern(left, _, AlwaysFalseCondition) ->
        left
    | LeftJoinPattern(left, FilterPattern(right, filter), condition) ->
        LeftJoinPattern(left, right, ConjunctionCondition([filter; condition]) |> normalizeSparqlCondition) |> normalizeSparqlPattern
    | LeftJoinPattern(_, _, _) ->
        input
    | NotMatchingPattern ->
        NotMatchingPattern
    | UnionPattern(unioned) ->
        let rec collectUnioned result = function
            | NotMatchingPattern :: xs -> collectUnioned result xs
            | UnionPattern(innerUnioned) :: xs -> collectUnioned result innerUnioned |> collectUnioned <| xs
            | x :: xs -> collectUnioned (x :: result) xs
            | [] -> result

        match collectUnioned [] unioned with
        | [] -> NotMatchingPattern
        | [i] -> i
        | _::_ as inner -> UnionPattern inner

    | NotProcessedTriplePatterns([]) ->
        EmptyPattern
    | NotProcessedTriplePatterns(_) ->
        input
    | RestrictedTriplePatterns([]) ->
        EmptyPattern
    | RestrictedTriplePatterns(_) ->
        input

// The desired order is SELECT -> DISTINCT -> ORDER BY -> SLICE
let normalizeModifiers (input: Modifier list) =
    fun current processed ->
        match current with
        | Select(_) ->
            match processed with
            | [] -> [current]
            | Select(_) :: _ -> raise (NotSupportedException("Two SELECT modifiers are not supported"))
            | _ -> current :: processed

        | Distinct ->
            match processed with
            | [] -> [current]
            | Select(_) as s :: n -> s :: current :: n
            | Distinct :: _ -> raise (NotSupportedException("Two DISTINCT modifiers are not supported"))
            | _ -> current :: processed

        | OrderBy _ ->
            match processed with
            | [] -> [current]
            | Select(_) as s :: n -> s :: current :: n
            | Distinct :: n -> Distinct :: current :: n
            | OrderBy(_) :: _ -> raise (NotSupportedException("Two ORDER BY modifiers are not supported"))
            | _ -> current :: processed

        | Slice sliceParams ->
            let modifiedSlice = 
                {|
                    Limit = if sliceParams.Limit = Some(0) then None else sliceParams.Limit
                    Offset = if sliceParams.Offset = Some(0) then None else sliceParams.Offset
                |}
                |> Slice

            match processed with
            | [] -> [modifiedSlice]
            | Select(_) as s :: n -> s :: modifiedSlice :: n
            | Distinct :: n -> Distinct :: modifiedSlice :: n
            | OrderBy(_) as o :: n -> o :: modifiedSlice :: n
            | Slice(_) :: _ -> raise (NotSupportedException("Two ORDER BY modifiers are not supported"))

    |> List.foldBack <|| (input, [])
