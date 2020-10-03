namespace Slp.Evi.Storage.Core.Common.Types

open System
open VDS.RDF
open Slp.Evi.Storage.Core.Common.Utils
open Slp.Evi.Storage.Core.Common
open Slp.Evi.Storage.Core.Common.ValueRestriction

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
    let xsdString = createFromXsd "#string"

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
            |> invalidOp
        | Some(l), _ ->
            l |> WithLanguage
        | _, Some(t) ->
            t |> Iri.fromUri |> WithType
        | _, _ ->
            DefaultType

    let private knownStringRestrictions =
        let choiceMachineForCharacters chars =
            chars
            |> List.map TemplateFsm.machineForCharacter
            |> TemplateFsm.choiceMachine

        let singleDigitCharacterMachine () =
            TemplateFsm.initialMachine ()
            |> TemplateFsm.appendEdge DigitCharacter

        let optionalSignCharacter () = 
            [
                TemplateFsm.machineForCharacter '+'
                TemplateFsm.machineForCharacter '-'
            ] |> TemplateFsm.choiceMachine |> TemplateFsm.optionalMachine

        let noDecimalPointNumeral () =
            singleDigitCharacterMachine ()
            |> TemplateFsm.atLeastOneRepeatMachine
            |> TemplateFsm.appendMachine <| optionalSignCharacter ()

        let decimalPointNumeral () =
            let afterDecimalPoint =
                singleDigitCharacterMachine ()
                |> TemplateFsm.atLeastOneRepeatMachine
                |> TemplateFsm.appendMachine <| TemplateFsm.machineForCharacter '.'

            singleDigitCharacterMachine ()
            |> TemplateFsm.infiniteRepeatMachine
            |> TemplateFsm.appendMachine <| optionalSignCharacter ()
            |> TemplateFsm.appendMachine afterDecimalPoint

        let numericalSpecialRep () =
            [
                TemplateFsm.machineForText "+INF"
                TemplateFsm.machineForText "INF"
                TemplateFsm.machineForText "-INF"
                TemplateFsm.machineForText "NaN"
            ] |> TemplateFsm.choiceMachine

        let scientificNotationNumeral () =
            [ noDecimalPointNumeral (); decimalPointNumeral () ]
            |> TemplateFsm.choiceMachine
            |> TemplateFsm.appendMachine (choiceMachineForCharacters [ 'e'; 'E' ])
            |> TemplateFsm.appendMachine (noDecimalPointNumeral ())

        let hexDigit () =
            "abcdefABCDEF"
            |> Seq.toList
            |> choiceMachineForCharacters

        let dateFrag () =
            TemplateFsm.initialMachine ()
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge (ExactCharacter '-')
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge (ExactCharacter '-')
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter

        let timeFrag () =
            TemplateFsm.initialMachine ()
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge (ExactCharacter ':')
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge (ExactCharacter ':')
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendEdge DigitCharacter
            |> TemplateFsm.appendMachine (
                singleDigitCharacterMachine ()
                |> TemplateFsm.atLeastOneRepeatMachine
                |> TemplateFsm.appendMachine <| TemplateFsm.machineForCharacter '.'
            )

        let timezoneFrag () =
            let timezone =
                [
                    TemplateFsm.machineForCharacter '+'
                    TemplateFsm.machineForCharacter '-'
                ]
                |> TemplateFsm.choiceMachine
                |> TemplateFsm.appendEdge DigitCharacter
                |> TemplateFsm.appendEdge DigitCharacter
                |> TemplateFsm.appendEdge (ExactCharacter ':')
                |> TemplateFsm.appendEdge DigitCharacter
                |> TemplateFsm.appendEdge DigitCharacter

            [
                TemplateFsm.machineForCharacter 'Z'
                timezone
            ]
            |> TemplateFsm.choiceMachine

        [
            (KnownTypes.xsdInteger, noDecimalPointNumeral ())
            (KnownTypes.xsdBoolean,
                [
                    TemplateFsm.machineForText "true"
                    TemplateFsm.machineForText "false"
                ]
                |> TemplateFsm.choiceMachine
            )
            (KnownTypes.xsdDecimal, [ noDecimalPointNumeral (); decimalPointNumeral () ] |> TemplateFsm.choiceMachine)
            (KnownTypes.xsdDouble, [ noDecimalPointNumeral (); decimalPointNumeral (); numericalSpecialRep (); scientificNotationNumeral () ] |> TemplateFsm.choiceMachine)
            (KnownTypes.xsdDate, timezoneFrag () |> TemplateFsm.optionalMachine |> TemplateFsm.appendMachine <| dateFrag ())
            (KnownTypes.xsdTime, timezoneFrag () |> TemplateFsm.optionalMachine |> TemplateFsm.appendMachine <| timeFrag ())
            (KnownTypes.xsdDateTime, dateFrag () |> TemplateFsm.appendEdge (ExactCharacter 'T') |> TemplateFsm.appendMachine (timeFrag ()) |> TemplateFsm.appendMachine (timezoneFrag () |> TemplateFsm.optionalMachine))
            (KnownTypes.xsdHexBinary, hexDigit () |> TemplateFsm.appendMachine (hexDigit ()) |> TemplateFsm.infiniteRepeatMachine)
        ]
        |> readOnlyDict

    let private nonIriTextRestriction =
        TemplateFsm.initialMachine ()
        |> TemplateFsm.appendEdge AnyCharacter
        |> TemplateFsm.infiniteRepeatMachine

    let private iriTextRestriction =
        TemplateFsm.initialMachine ()
        |> TemplateFsm.appendEdge IriUnRestrictedCharacter
        |> TemplateFsm.infiniteRepeatMachine

    let valueStringRestriction (isIri: bool) (input: LiteralValueType) =
        match knownStringRestrictions.TryGetValue input with
        | true, restriction -> restriction
        | false, _ -> if isIri then iriTextRestriction else nonIriTextRestriction

type NodeType =
    | BlankNodeType
    | IriNodeType
    | LiteralNodeType of LiteralValueType