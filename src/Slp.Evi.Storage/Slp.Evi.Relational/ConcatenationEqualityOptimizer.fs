module Slp.Evi.Relational.ConcatenationEqualityOptimizer

open Slp.Evi.Relational.Algebra
open TCode.r2rml4net

type ConcatenationPattern = Expression list

type ConcatenationEqualityCondition =
    | AlwaysMatching
    | AlwaysNotMatching
    | MatchingCondition of ConcatenationPattern * ConcatenationPattern

type ConcatenationEqualityResult = ConcatenationEqualityCondition list

type private ConcatenationSimplePatternPart =
    | ConcatVariable of Variable
    | ConcatIriSafeVariable of Variable
    | ConcatCharacter of char

type private ConcatenationSimplePattern =
    ConcatenationSimplePatternPart list

type private ConcatenationSimpleEqualityCondition =
    | SimpleAlwaysMatching
    | SimpleAlwaysNotMatching
    | SimpleMatchingCondition of ConcatenationSimplePattern * ConcatenationSimplePattern

type private ConcatenationSimpleEqualityResult = ConcatenationSimpleEqualityCondition list

let private explodeConcatenationToConcatCharacters (concatenation: ConcatenationPattern) =
    (concatenation, List.empty |> Some)
    ||> List.foldBack (
        fun cur maybeRest ->
            maybeRest
            |> Option.bind (
                fun rest ->
                    match cur with
                    | Variable v ->
                        (v |> ConcatVariable) :: rest
                        |> Some
                    | IriSafeVariable v ->
                        (v |> ConcatIriSafeVariable) :: rest
                        |> Some
                    | Constant(c) ->
                        match c with
                        | Int i -> i.ToString()
                        | Double d -> d.ToString()
                        | String s -> s
                        | DateTimeLiteral d -> System.Xml.XmlConvert.ToString (d, System.Xml.XmlDateTimeSerializationMode.Utc)
                        |> Seq.map ConcatCharacter
                        |> Seq.toList
                        |> fun x -> x @ rest
                        |> Some
                    | _ ->
                        None
            )
    )

let private isIReserved = MappingHelper.IsIUnreserved >> not

let private isIriSafeAdd =
    function
    | ConcatVariable _ -> false
    | _ -> true

let rec private buildConcatenationEqualityInDirection (isAccIriSafe: bool) (currentResult: ConcatenationSimpleEqualityResult) (leftAcc: ConcatenationSimplePattern) (rightAcc: ConcatenationSimplePattern) (leftTemplate: ConcatenationSimplePattern) (rightTemplate: ConcatenationSimplePattern): ConcatenationSimpleEqualityResult =
    match (leftTemplate, rightTemplate, leftAcc, rightAcc) with
    | ConcatCharacter l :: xleft, ConcatCharacter r :: xright, [], [] ->
        if l = r then
            buildConcatenationEqualityInDirection isAccIriSafe currentResult [] [] xleft xright
        else
            SimpleAlwaysNotMatching |> List.singleton
    | ConcatCharacter l :: xleft, ConcatCharacter r :: xright, _, _ when isAccIriSafe && isIReserved l && isIReserved r ->
        if l = r then
            buildConcatenationEqualityInDirection true (((leftAcc, rightAcc) |> SimpleMatchingCondition) :: currentResult) [] [] xleft xright
        else
            SimpleAlwaysNotMatching |> List.singleton
    | ConcatCharacter l :: _, r :: xright, _, _ when isAccIriSafe && isIReserved l ->
        buildConcatenationEqualityInDirection (isIriSafeAdd r) currentResult leftAcc (r :: rightAcc) leftTemplate xright
    | l :: xleft, ConcatCharacter r :: _, _, _ when isAccIriSafe && isIReserved r ->
        buildConcatenationEqualityInDirection (isIriSafeAdd l) currentResult (l :: leftAcc) rightAcc xleft rightTemplate
    | l :: xleft, _, _, _ ->
        buildConcatenationEqualityInDirection (isIriSafeAdd l) currentResult (l :: leftAcc) rightAcc xleft rightTemplate
    | _, r :: xright, _, _ ->
        buildConcatenationEqualityInDirection (isIriSafeAdd r) currentResult leftAcc (r :: rightAcc) leftTemplate xright
    | [], [], [], [] ->
        currentResult
    | [], [], _, _ ->
        ((leftAcc, rightAcc) |> SimpleMatchingCondition) :: currentResult

let rec private finalizeTemplate (acc: char list) (result: ConcatenationPattern) (input: ConcatenationSimplePattern) =
    let templateFromAcc () =
        acc
        |> List.rev
        |> Array.ofList
        |> System.String
        |> String
        |> Constant

    match input, acc with
    | ConcatVariable v :: rest, [] ->
        finalizeTemplate [] ((v |> Variable) :: result) rest
    | ConcatIriSafeVariable v :: rest, [] ->
        finalizeTemplate [] ((v |> IriSafeVariable) :: result) rest
    | ConcatVariable _ :: _, _
    | ConcatIriSafeVariable _ :: _, _ ->
        finalizeTemplate [] (templateFromAcc () :: result) input
    | ConcatCharacter c :: rest, _ ->
        finalizeTemplate (c :: acc) result rest
    | [], [] ->
        result |> List.rev
    | [], _ ->
        finalizeTemplate [] (templateFromAcc () :: result) []

let rec private revertResult (acc: ConcatenationSimpleEqualityResult) (input: ConcatenationSimpleEqualityResult) =
    match input with
    | SimpleMatchingCondition(left, right) :: xs ->
        let leftRev = left |> List.rev
        let rightRev = right |> List.rev
        let revCondition = SimpleMatchingCondition(leftRev, rightRev)
        revertResult (revCondition :: acc) xs
    | x :: xs -> revertResult (x :: acc) xs
    | [] -> acc

let rec private finalizeResult (result: ConcatenationEqualityResult) (input: ConcatenationSimpleEqualityResult) =
    match input with
    | SimpleAlwaysMatching :: rest -> finalizeResult result rest
    | SimpleAlwaysNotMatching :: _ -> [ AlwaysNotMatching ]
    | SimpleMatchingCondition(left, right) :: rest ->
        let newLeft = left |> finalizeTemplate [] []
        let newRight = right |> finalizeTemplate [] []

        match newLeft, newRight with
        | [], [] ->
            finalizeResult result rest
        | [], nonEmpty
        | nonEmpty, [] ->
            let isNonEmptyString =
                function
                | Constant(String(x)) when x |> String.length > 0 -> true
                | _ -> false

            if nonEmpty |> List.exists isNonEmptyString then
                [ AlwaysNotMatching ]
            else
                MatchingCondition(newLeft, newRight) :: result |> finalizeResult <| rest
        | _, _ ->
            MatchingCondition(newLeft, newRight) :: result |> finalizeResult <| rest

    | [] -> result |> List.rev

let compareConcatenations (leftTemplate: ConcatenationPattern) (rightTemplate: ConcatenationPattern) =
    let maybeLeftExploded = leftTemplate |> explodeConcatenationToConcatCharacters
    let maybeRightExploded = rightTemplate |> explodeConcatenationToConcatCharacters

    match maybeLeftExploded, maybeRightExploded with
    | Some(leftExploded), Some(rightExploded) ->
        let processedFromFront =
            buildConcatenationEqualityInDirection true [ SimpleAlwaysMatching ] [] [] leftExploded rightExploded

        let processedFromBack =
            match processedFromFront with
            | SimpleMatchingCondition(leftEnd, rightEnd) :: other ->
                let processedFromBack = 
                    buildConcatenationEqualityInDirection true [ SimpleAlwaysMatching ] [] [] leftEnd rightEnd

                (other |> revertResult []) @ processedFromBack
            | _ -> processedFromFront |> revertResult []

        processedFromBack
        |> finalizeResult []
        |> Some

    | _ ->
        None
