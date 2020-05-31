namespace Slp.Evi.Common.Types

open System
open VDS.RDF
open Slp.Evi.Common.Utils

type LiteralValueType =
    | DefaultType
    | WithType of Uri
    | WithLanguage of string

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
            t |> WithType
        | _, _ ->
            DefaultType

type NodeType =
    | BlankNode
    | IriNode
    | LiteralNode of LiteralValueType

module KnownTypes =
    let private baseXsdNamespace = "http://www.w3.org/2001/XMLSchema#"
    let private createFromXsd name =
        sprintf "%s%s" baseXsdNamespace name
        |> Uri
        |> WithType

    let xsdInteger = createFromXsd "integer"
    let xsdBoolean = createFromXsd "boolean"
    let xsdDecimal = createFromXsd "decimal"
    let xsdDouble = createFromXsd "double"
    let xsdDate = createFromXsd "date"
    let xsdTime = createFromXsd "time"
    let xsdDateTime = createFromXsd "dateTime"
    let xsdHexBinary = createFromXsd "hexBinary"
