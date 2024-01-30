namespace Slp.Evi.Storage.Core.Relational

open System.Collections.Concurrent
open System.Threading

open Slp.Evi.Storage.Core.Common.Types

module TypeIndexer =
    type TypeCategory =
        | BlankNode = 0
        | Iri = 1
        | SimpleLiteral = 2
        | NumericLiteral = 3
        | StringLiteral = 4
        | BooleanLiteral = 5
        | DateTimeLiteral = 6
        | OtherLiteral = 7

    type TypeRecord = { Index: int64; Category: TypeCategory; NodeType: NodeType }

    let private knownTypeToCategoryMappings =
        [
            KnownTypes.xsdBoolean, TypeCategory.BooleanLiteral
            KnownTypes.xsdDate, TypeCategory.DateTimeLiteral
            KnownTypes.xsdDateTime, TypeCategory.DateTimeLiteral
            KnownTypes.xsdTime, TypeCategory.DateTimeLiteral
            KnownTypes.xsdDecimal, TypeCategory.NumericLiteral
            KnownTypes.xsdDouble, TypeCategory.NumericLiteral
            KnownTypes.xsdInteger, TypeCategory.NumericLiteral
            KnownTypes.xsdString, TypeCategory.StringLiteral
        ]
        |> readOnlyDict

    let categoryFromNodeType nodeType =
        match nodeType with
        | BlankNodeType -> TypeCategory.BlankNode
        | IriNodeType -> TypeCategory.Iri
        | LiteralNodeType DefaultType -> TypeCategory.SimpleLiteral
        | LiteralNodeType (WithLanguage _) -> TypeCategory.OtherLiteral
        | LiteralNodeType (WithType t) ->
            match knownTypeToCategoryMappings.TryGetValue (WithType t) with
            | true, cat -> cat
            | false, _ -> TypeCategory.OtherLiteral

type TypeIndexer () =
    let indexedRecords = ConcurrentDictionary<int64, TypeIndexer.TypeRecord>()
    let typeToIndex = ConcurrentDictionary<NodeType, int64>()
    let largestIndex = ref 0L

    let getTypeRecord nodeType =
        let createNewIndex _ =
            Interlocked.Increment largestIndex

        let createNewRecordType nodeType index =
            {
                TypeIndexer.TypeRecord.Index =
                    index
                TypeIndexer.TypeRecord.Category =
                    nodeType
                    |> TypeIndexer.categoryFromNodeType
                TypeIndexer.TypeRecord.NodeType =
                    nodeType
            }

        let index = typeToIndex.GetOrAdd(nodeType, createNewIndex)
        indexedRecords.GetOrAdd(index, createNewRecordType nodeType)

    let getFromIndex index =
        match indexedRecords.TryGetValue index with
        | true, record -> record
        | false, _ ->
            sprintf "Requested an entry for index %d which was not inserted before" index
            |> invalidArg (nameof index)

    member _.FromType nodeType = getTypeRecord nodeType
    member _.FromIndex index = getFromIndex index
    member _.IndexedTypes with get () =
        indexedRecords.Values |> Seq.toList
