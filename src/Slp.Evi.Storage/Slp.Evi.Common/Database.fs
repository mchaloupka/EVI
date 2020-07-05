namespace Slp.Evi.Common.Database

open Slp.Evi.Common.Types

type ISqlColumnType =
    abstract member DefaultRdfType: LiteralValueType with get

    abstract member IsNullable: bool with get

type ISqlColumnSchema =
    abstract member Name: string with get

    abstract member SqlType: ISqlColumnType with get

type ISqlTableSchema =
    abstract member Name: string with get

    abstract member GetColumn: columnName: string -> ISqlColumnSchema

    abstract member Columns: string seq with get

    abstract member Keys: string seq seq with get

type ISqlDatabaseSchema =
    abstract member GetTable: tableName: string -> ISqlTableSchema

type ISqlDatabase =
    abstract member DatabaseSchema: ISqlDatabaseSchema with get

    abstract member ExecuteQuery: query:string -> unit