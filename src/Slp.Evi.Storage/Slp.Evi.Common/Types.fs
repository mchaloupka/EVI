namespace Slp.Evi.Common.Types

open System
open VDS.RDF
open Slp.Evi.Common.Utils
open Slp.Evi.Common.StringRestriction
open Slp.Evi.Common

type LiteralValueType =
    | DefaultType
    | WithType of Iri
    | WithLanguage of string

module KnownTypes =
    let private baseXsdIri = "http://www.w3.org/2001/XMLSchema" |> Uri |> Iri.fromUri
    let private createFromXsd = IriReference.fromString >> IriReference.resolve baseXsdIri >> WithType

    let xsdInteger = createFromXsd "#integer"
    let xsdBoolean = createFromXsd "#boolean"
    let xsdDecimal = createFromXsd "#decimal"
    let xsdDouble = createFromXsd "#double"
    let xsdDate = createFromXsd "#date"
    let xsdTime = createFromXsd "#time"
    let xsdDateTime = createFromXsd "#dateTime"
    let xsdHexBinary = createFromXsd "#hexBinary"

module LiteralValueType =
    let fromLiteralNode (node: ILiteralNode) =
        let literalType = node.DataType |> createOptionFromNullable
        let language =
            if node.Language |> String.IsNullOrEmpty then
                None
            else
                Some(node.Language)

        match (language, literalType) with
        | Some(l), Some(t) ->
            sprintf "Node %A have both language %A and type %A" node l t
            |> InvalidOperationException
            |> raise
        | Some(l), _ ->
            l |> WithLanguage
        | _, Some(t) ->
            t |> Iri.fromUri |> WithType
        | _, _ ->
            DefaultType

    let private knownStringRestrictions =
        let optionalSignCharacter = 
            [
                [ExactCharacter '+']
                [ExactCharacter '-']
            ] |> Choice |> List.singleton |> Optional

        let noDecimalPointNumeral =
            [
                optionalSignCharacter
                [ DigitCharacter ] |> AtLeastOneRepetition
            ]

        let decimalPointNumeral =
            [
                optionalSignCharacter
                [ DigitCharacter ] |> InfiniteRepetition
                ExactCharacter '.'
                [ DigitCharacter ] |> AtLeastOneRepetition
            ]

        let numericalSpecialRep =
            [
                RestrictedTemplate.fromText "+INF"
                RestrictedTemplate.fromText "INF"
                RestrictedTemplate.fromText "-INF"
                RestrictedTemplate.fromText "NaN"
            ] |> Choice |> List.singleton

        let scientificNotationNumeral =
            [
                [ noDecimalPointNumeral; decimalPointNumeral ] |> Choice
                RestrictedTemplate.fromText "eE" |> List.map List.singleton |> Choice
            ] @ noDecimalPointNumeral

        let hexDigit =
            DigitCharacter :: RestrictedTemplate.fromText "abcdefABCDEF" 
            |> List.map List.singleton 
            |> Choice

        let dateFrag = 
            [
                DigitCharacter
                DigitCharacter
                DigitCharacter
                DigitCharacter
                ExactCharacter '-'
                DigitCharacter
                DigitCharacter
                ExactCharacter '-'
                DigitCharacter
                DigitCharacter
            ]

        let timeFrag =
            [
                DigitCharacter
                DigitCharacter
                ExactCharacter ':'
                DigitCharacter
                DigitCharacter
                ExactCharacter ':'
                DigitCharacter
                DigitCharacter
                [
                    ExactCharacter '.'
                    DigitCharacter |> List.singleton |> AtLeastOneRepetition
                ] |> Optional
            ]

        let timezoneFrag =
            [
                ExactCharacter 'Z' |> List.singleton
                [
                    [
                        [ExactCharacter '+']
                        [ExactCharacter '-']
                    ] |> Choice
                    DigitCharacter
                    DigitCharacter
                    ExactCharacter ':'
                    DigitCharacter
                    DigitCharacter
                ]
            ] |> Choice |> List.singleton

        [
            (KnownTypes.xsdInteger, noDecimalPointNumeral)
            (KnownTypes.xsdBoolean, [
                [
                    RestrictedTemplate.fromText "true"
                    RestrictedTemplate.fromText "false"
                ] |> Choice
            ])
            (KnownTypes.xsdDecimal, [ noDecimalPointNumeral; decimalPointNumeral ] |> Choice |> List.singleton)
            (KnownTypes.xsdDouble, [ noDecimalPointNumeral; decimalPointNumeral; numericalSpecialRep; scientificNotationNumeral ] |> Choice |> List.singleton)
            (KnownTypes.xsdDate, dateFrag @ (timezoneFrag |> Optional |> List.singleton))
            (KnownTypes.xsdTime, timeFrag @ (timezoneFrag |> Optional |> List.singleton))
            (KnownTypes.xsdDateTime, dateFrag @ [ ExactCharacter 'T' ] @ timeFrag @ (timezoneFrag |> Optional |> List.singleton))
            (KnownTypes.xsdHexBinary, [
                [
                    hexDigit
                    hexDigit
                ] |> InfiniteRepetition
            ])
        ]
        |> readOnlyDict

    let valueStringRestriction (input: LiteralValueType) =
        match knownStringRestrictions.TryGetValue input with
        | true, restriction -> restriction
        | false, _ -> AnyCharacter |> List.singleton |> InfiniteRepetition |> List.singleton

type NodeType =
    | BlankNodeType
    | IriNodeType
    | LiteralNodeType of LiteralValueType