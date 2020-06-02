namespace Slp.Evi.Common.StringRestriction

type RestrictedTemplate =
    RestrictedTemplateElement list
and RestrictedTemplateElement =
    | AnyCharacter
    | DigitCharacter
    | ExactCharacter of char
    | InfiniteRepetition of RestrictedTemplate
    | AtLeastOneRepetition of RestrictedTemplate
    | Choice of RestrictedTemplate list
    | Optional of RestrictedTemplate

module RestrictedTemplate =
    let rec private matchingReducer left right =
        match left, right with
        | [], [] ->
            ([], []) |> Seq.singleton
        | AnyCharacter :: _, []
        | [], AnyCharacter :: _
        | DigitCharacter :: _, []
        | [], DigitCharacter :: _
        | ExactCharacter _ :: _, []
        | [], ExactCharacter _ :: _ ->
            Seq.empty
        | AnyCharacter :: ls, AnyCharacter :: rs
        | AnyCharacter :: ls, DigitCharacter _ :: rs
        | DigitCharacter _ :: ls, AnyCharacter :: rs        
        | DigitCharacter :: ls, DigitCharacter _ :: rs
        | AnyCharacter :: ls, ExactCharacter _ :: rs
        | ExactCharacter _ :: ls, AnyCharacter :: rs ->
            matchingReducer ls rs
        | DigitCharacter :: xs, ExactCharacter c :: os
        | ExactCharacter c :: os, DigitCharacter :: xs ->
            if System.Char.IsDigit(c) then
                matchingReducer xs os
            else
                Seq.empty
        | ExactCharacter l :: ls, ExactCharacter r :: rs ->
            if l = r then
                matchingReducer ls rs
            else
                Seq.empty
        | AtLeastOneRepetition x :: xs, os
        | os, AtLeastOneRepetition x :: xs ->
            x @ InfiniteRepetition x :: xs
            |> matchingReducer os
        | Optional opt :: xs, os
        | os, Optional opt :: xs ->
            [
                matchingReducer xs os
                matchingReducer (opt @ xs) os
            ]
            |> Seq.concat
        | Choice choices :: xs, os
        | os, Choice choices :: xs ->
            choices
            |> Seq.collect (
                fun x ->
                    x @ xs
                    |> matchingReducer os
            )
        | (InfiniteRepetition _ :: _ as l), (InfiniteRepetition _ :: _ as r) ->
            (l, r) |> Seq.singleton
        | InfiniteRepetition x :: xs, os
        | os, InfiniteRepetition x :: xs ->
            [
                matchingReducer xs os
                matchingReducer (x @ InfiniteRepetition x :: xs) os
            ]
            |> Seq.concat

    let canMatch left right =
        matchingReducer left right
        |> Seq.collect (fun (l, r) -> matchingReducer l r)
        |> Seq.isEmpty
        |> not

    let fromText text =
        text
        |> List.ofSeq
        |> List.map ExactCharacter