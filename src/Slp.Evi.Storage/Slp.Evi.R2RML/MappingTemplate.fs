module Slp.Evi.R2RML.MappingTemplate

open System.Text.RegularExpressions
open TCode.r2rml4net
open System

type TemplatePart<'T> =
    | ColumnPart of 'T
    | TextPart of string

type Template<'T> = TemplatePart<'T> list

let private templateReplaceRegex = new Regex(@"(?<N>\{)([^\{\}.]+)(?<-N>\})(?(N)(?!))", RegexOptions.Compiled)

let parseTemplate (columnRetrieval: string -> 'T) (template: string) =
    let processMatch (matchIndex, length) (next, endIndex) =
        let column = template.Substring(matchIndex + 1, length - 2) |> columnRetrieval |> ColumnPart
        let matchEnd = matchIndex + length
        if matchEnd < endIndex then
            let text = template.Substring(matchEnd, endIndex - matchEnd) |> TextPart
            column :: text :: next, matchIndex
        else
            column :: next, matchIndex
    
    let (result, startIndex) =
        templateReplaceRegex.Matches(template)
        |> Seq.cast<Match>
        |> Seq.sortBy (fun x -> x.Index)
        |> Seq.map(fun x -> x.Index, x.Length)
        |> Seq.foldBack processMatch <| ([], template.Length)

    if startIndex > 0 then
        let start = template.Substring(0, startIndex) |> TextPart
        start :: result
    else
        result

type TemplateMatchCondition<'T> =
    | AlwaysMatching
    | AlwaysNotMatching
    | MatchingCondition of Template<'T> * Template<'T>

type TemplateCompareResult<'T> = TemplateMatchCondition<'T> list

module TemplateCompareResult =
    let isAlwaysMatching (result: TemplateCompareResult<'T>) =
        result
        |> List.forall (
            function
            | AlwaysMatching -> true
            | _ -> false
        )

    let isNeverMatching (result: TemplateCompareResult<'T>) =
        result
        |> List.exists (
            function
            | AlwaysNotMatching -> true
            | _ -> false
        )

let rec private explodeTemplateToCharacters (template: Template<'T>) =
    match template with
    | ColumnPart(_) as c :: xs -> c :: explodeTemplateToCharacters xs
    | TextPart(text) :: xs ->
        let exploded =
            text |> Seq.map (fun c -> c.ToString()) |> Seq.map TextPart |> List.ofSeq

        exploded @ explodeTemplateToCharacters xs
    | [] -> []

let rec private isIReserved s =
    if s |> String.length = 1 then
        s.[0] |> MappingHelper.IsIUnreserved |> not
    else
        InvalidOperationException("At this moment, there should not be any text part with different length than 1") |> raise

let rec private buildTemplateComparisonInDirection (isIriMatch: bool) (currentResult: TemplateCompareResult<'T>) (leftAcc: Template<'T>) (rightAcc: Template<'T>) (leftTemplate: Template<'T>) (rightTemplate: Template<'T>): TemplateCompareResult<'T> =
    match (leftTemplate, rightTemplate, leftAcc, rightAcc) with
    | TextPart l :: xleft, TextPart r :: xright, [], [] ->
        if l = r then
            buildTemplateComparisonInDirection isIriMatch currentResult [] [] xleft xright
        else
            [ AlwaysNotMatching ]
    | TextPart l :: xleft, TextPart r :: xright, _, _ when isIriMatch && isIReserved l && isIReserved r ->
        if l = r then
            buildTemplateComparisonInDirection isIriMatch (((leftAcc, rightAcc) |> MatchingCondition) :: currentResult) [] [] xleft xright
        else
            [ AlwaysNotMatching ]
    | TextPart l :: _, r :: xright, _, _ when isIriMatch && isIReserved l ->
        buildTemplateComparisonInDirection isIriMatch currentResult leftAcc (r :: rightAcc) leftTemplate xright
    | l :: xleft, TextPart r :: _, _, _ when isIriMatch && isIReserved r ->
        buildTemplateComparisonInDirection isIriMatch currentResult (l :: leftAcc) rightAcc xleft rightTemplate
    | l :: xleft, _, _, _ ->
        buildTemplateComparisonInDirection isIriMatch currentResult (l :: leftAcc) rightAcc xleft rightTemplate
    | _, r :: xright, _, _ ->
        buildTemplateComparisonInDirection isIriMatch currentResult leftAcc (r :: rightAcc) leftTemplate xright
    | [], [], [], [] ->
        currentResult
    | [], [], _, _ ->
        ((leftAcc, rightAcc) |> MatchingCondition) :: currentResult

let private finalizeResult (input: TemplateCompareResult<'T>) =
    input

let rec private revertResult (acc: TemplateCompareResult<'T>) (input: TemplateCompareResult<'T>) =
    match input with
    | MatchingCondition(left, right) :: xs ->
        let leftRev = left |> List.rev
        let rightRev = right |> List.rev
        let revCondition = MatchingCondition(leftRev, rightRev)
        revertResult (revCondition :: acc) xs
    | x :: xs -> revertResult (x :: acc) xs
    | [] -> acc

let compareTemplates (isIriMatch: bool) (leftTemplate: Template<'T>) (rightTemplate: Template<'T>) =
    let leftExploded = leftTemplate |> explodeTemplateToCharacters
    let rightExploded = rightTemplate |> explodeTemplateToCharacters
    
    let processedFromFront =
        buildTemplateComparisonInDirection isIriMatch [ AlwaysMatching ] [] [] leftExploded rightExploded

    let processedFromBack =
        match processedFromFront with
        | MatchingCondition(leftEnd, rightEnd) :: other ->
            let processedFromBack = 
                buildTemplateComparisonInDirection isIriMatch [ AlwaysMatching ] [] [] leftEnd rightEnd

            (other |> revertResult []) @ processedFromBack
        | _ -> processedFromFront |> revertResult []

    processedFromBack
    |> finalizeResult


