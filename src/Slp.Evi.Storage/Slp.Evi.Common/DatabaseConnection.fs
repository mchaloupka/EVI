namespace Slp.Evi.Common.DatabaseConnection

open System.Data.Common

type ISqlDatabaseConnection =
    abstract member ExecuteQuery: query:string -> unit

    abstract member GetRawConnection: unit -> DbConnection

type ISqlDatabase =
    abstract member GetConnection: unit -> ISqlDatabaseConnection