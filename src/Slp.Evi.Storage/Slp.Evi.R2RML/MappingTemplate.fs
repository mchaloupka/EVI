module Slp.Evi.R2RML.MappingTemplate

open System.Text.RegularExpressions

type TemplatePart =
    | ColumnPart of string
    | TextPart of string

type Template = TemplatePart list

let private templateReplaceRegex = new Regex(@"(?<N>\{)([^\{\}.]+)(?<-N>\})(?(N)(?!))", RegexOptions.Compiled)

let parseTemplate (template: string) =
    let processMatch (matchIndex, length) (next, endIndex) =
        let column = template.Substring(matchIndex + 1, length - 2) |> ColumnPart
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
