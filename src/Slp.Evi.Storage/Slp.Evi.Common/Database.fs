namespace Slp.Evi.Common.Database

open Slp.Evi.Common.Types

type ISqlColumnType =
    abstract member DefaultRdfType: LiteralValueType with get

type ISqlColumnSchema =
    abstract member Name: string with get

    abstract member SqlType: ISqlColumnType with get

type ISqlTableSchema =
    abstract member Name: string with get

    abstract member GetColumn: columnName: string -> ISqlColumnSchema

type ISqlDatabaseSchema =
    abstract member GetTable: tableName: string -> ISqlTableSchema

type ISqlDatabase =
    abstract member DatabaseSchema: ISqlDatabaseSchema with get

    abstract member ExecuteQuery: query:string -> unit