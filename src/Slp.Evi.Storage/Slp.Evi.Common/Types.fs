module Slp.Evi.Common.Types

open System

type LiteralValueType =
    | DefaultType
    | WithType of Uri
    | WithLanguage of string

type NodeType =
    | BlankNode
    | IriNode
    | LiteralNode of LiteralValueType
