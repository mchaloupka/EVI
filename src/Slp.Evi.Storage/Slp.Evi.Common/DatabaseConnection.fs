namespace Slp.Evi.Common.DatabaseConnection

open System.Data.Common
open Slp.Evi.Common.Types

type ISqlDatabaseSchema =
    abstract member NormalizeTableName: tableName: string -> string

    abstract member DetectDefaultRdfType: tableName: string * columnName: string -> LiteralValueType

type ISqlDatabase =
    abstract member DatabaseSchema: ISqlDatabaseSchema with get

    abstract member ExecuteQuery: query:string -> unit