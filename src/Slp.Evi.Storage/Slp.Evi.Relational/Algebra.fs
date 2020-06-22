namespace Slp.Evi.Relational.Algebra

open System
open DatabaseSchemaReader.DataSchema
open Slp.Evi.Common.Algebra

type SqlColumn = { Name: string; Source: ISqlSource; Type: DataType }

and ISqlSource =
    abstract member GetColumn: string -> SqlColumn

type Literal =
    | String of string
    | Int of int
    | Double of double
    | DateTime of DateTime

type AssignedVariable(dataType: DataType) =
    member this.DataType = dataType

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
    | Sql of ISqlSource
    | Model of CalculusModel
    | ModifiedModel of ModifiedCalculusModel
    | UnionModel of Variable * Source list
    | LeftOuterJoinModel of Source * Condition

and Source = { Variables: Variable list; VariableSource: VariableSource }

and CalculusModel = { Sources: Source list; Assignments: Assignment list; Filter: Condition list }

and ModifiedCalculusModel = { InnerModel: CalculusModel; Ordering: Ordering list; Limit: int option; Offset: int option; IsDistinct: bool }