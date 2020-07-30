namespace Slp.Evi.Database

open Slp.Evi.Common.Database

type ISqlResultColumn<'C> =
    abstract member Name: 'C with get

type ISqlResultRow<'C> =
    abstract member Columns: ISqlResultColumn<'C> seq with get

    abstract member GetColumn: 'C -> ISqlResultColumn<'C>

type ISqlResultReader<'C> =
    abstract member HasNextRow: bool with get

    abstract member ReadRow: unit -> ISqlResultRow<'C>

type ISqlDatabase<'T, 'C> =
    abstract member DatabaseSchema: ISqlDatabaseSchema with get

    abstract member ExecuteQuery: query: 'T -> ISqlResultReader<'C>
