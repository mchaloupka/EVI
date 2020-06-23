namespace Slp.Evi.Relational.Algebra

open System
open DatabaseSchemaReader.DataSchema
open Slp.Evi.Common.Algebra
open Slp.Evi.Common.Database

[<ReferenceEquality>]
type SqlColumn = { Schema: ISqlColumnSchema; }

[<ReferenceEquality>]
type SqlSource = { Schema: ISqlTableSchema; Columns: SqlColumn list }

type Literal =
    | String of string
    | Int of int
    | Double of double
    | DateTime of DateTime

type AssignedVariable(dataType: DataType) =
    member _.DataType = dataType

type Variable =
    | Assigned of AssignedVariable
    | Column of SqlColumn

type Condition =
    | AlwaysFalse
    | AlwaysTrue
    | Comparison of Comparisons * Expression * Expression
    | Conjunction of Condition list
    | Disjunction of Condition list
    | EqualVariables of Variable * Variable
    | IsNull of Variable
    | LanguageMatch of Expression * Expression
    | Like of Expression * string
    | Not of Condition

and CaseStatement = { Condition: Condition; Expression: Expression }

and Expression =
    | BinaryNumericOperation of ArithmeticOperator * Expression * Expression
    | Switch of CaseStatement list
    | Coalesce of Expression list
    | Variable of Variable
    | Constant of Literal
    | Null

type Assignment = { Variable: AssignedVariable; Expression: Expression }

type Ordering = { Expression: Expression; Direction: OrderingDirection }

type VariableSource =
    | NoResult
    | SingleEmptyResult
    | Sql of SqlSource
    | ModifiedModel of ModifiedCalculusModel
    | UnionModel of Variable * CalculusModel list
    | LeftOuterJoinModel of CalculusModel * Condition

and CalculusModel = { Sources: VariableSource list; Assignments: Assignment list; Filters: Condition list }

and ModifiedCalculusModel = { InnerModel: CalculusModel; Ordering: Ordering list; Limit: int option; Offset: int option; IsDistinct: bool }