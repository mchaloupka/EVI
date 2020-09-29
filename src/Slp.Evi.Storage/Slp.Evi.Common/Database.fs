namespace Slp.Evi.Common.Database

open Slp.Evi.Common.Types

type ISqlColumnType =
    abstract member DefaultRdfType: LiteralValueType with get

type SqlColumnSchema = { Name: string; IsNullable: bool; SqlType: ISqlColumnType }

type ISqlTableSchema =
    abstract member Name: string with get

    abstract member GetColumn: columnName: string -> SqlColumnSchema

    abstract member Columns: string seq with get

    abstract member Keys: string seq seq with get

type ISqlDatabaseSchema =
    abstract member GetTable: tableName: string -> ISqlTableSchema

    abstract member NullType: ISqlColumnType with get

    abstract member IntegerType: ISqlColumnType with get

    abstract member StringType: ISqlColumnType with get

    abstract member DoubleType: ISqlColumnType with get

    abstract member BooleanType: ISqlColumnType with get

    abstract member DateTimeType: ISqlColumnType with get

    abstract member GetCommonType: left: ISqlColumnType * right: ISqlColumnType -> ISqlColumnType