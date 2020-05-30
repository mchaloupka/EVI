namespace Slp.Evi.Common.Types

open System

type LiteralValueType =
    | DefaultType
    | WithType of Uri
    | WithLanguage of string

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
