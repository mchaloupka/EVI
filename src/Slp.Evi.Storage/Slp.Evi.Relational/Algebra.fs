namespace Slp.Evi.Relational.Algebra

open System
open DatabaseSchemaReader.DataSchema
open Slp.Evi.Common.Algebra
open Slp.Evi.Common.Database
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Common.Types
open Slp.Evi.R2RML

[<ReferenceEquality>]
type SqlColumn = { Schema: ISqlColumnSchema; }

[<ReferenceEquality>]
type SqlSource = { Schema: ISqlTableSchema; Columns: SqlColumn list }

module SqlSource =
    let getColumn name source =
        source.Columns |> List.find (fun x -> x.Schema.Name = name)

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
    | Sql of SqlSource
    | SubQuery of CalculusModel
    | UnionModel of Variable * NotModifiedCalculusModel list
    | LeftOuterJoinModel of NotModifiedCalculusModel * Condition

and NotModifiedCalculusModel = { NonNullVariables: Variable list; Sources: VariableSource list; Assignments: Assignment list; Filters: Condition list }

and ModifiedCalculusModel = { InnerModel: CalculusModel; Ordering: Ordering list; Limit: int option; Offset: int option; IsDistinct: bool }

and CalculusModel =
    | NoResult
    | SingleEmptyResult
    | Modified of ModifiedCalculusModel
    | NotModified of NotModifiedCalculusModel

type ValueBinder =
    | EmptyValueBinder
    | BaseValueBinder of ObjectMapping * Map<string, Variable>
    | CoalesceValueBinder of ValueBinder list
    | CaseValueBinder of Variable * Map<int, ValueBinder>
    | ExpressionValueBinder of Expression * NodeType

type BoundCalculusModel = { Model: CalculusModel; Bindings: Map<SparqlVariable, ValueBinder>; AlwaysBoundVariables: SparqlVariable list }