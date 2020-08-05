namespace Slp.Evi.Database

open Slp.Evi.Common.Database
open System

type ISqlResultColumn =
    abstract member Name: string with get

type ISqlResultRow =
    abstract member Columns: ISqlResultColumn seq with get

    abstract member GetColumn: string -> ISqlResultColumn

type ISqlResultReader =
    abstract member HasNextRow: bool with get

    abstract member ReadRow: unit -> ISqlResultRow

    inherit IDisposable
    
type ISqlDatabaseWriter<'T> =
    abstract member WriteQuery: query: SqlQuery -> 'T

type ISqlDatabase<'T> =
    abstract member DatabaseSchema: ISqlDatabaseSchema with get

    abstract member Writer: ISqlDatabaseWriter<'T> with get

    abstract member ExecuteQuery: query: 'T -> ISqlResultReader
