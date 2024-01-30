﻿namespace Slp.Evi.Storage.Core.Relational.Algebra

open System
open Slp.Evi.Storage.Core.Common.Algebra
open Slp.Evi.Storage.Core.Common.Database
open Slp.Evi.Storage.Core.Sparql.Algebra
open Slp.Evi.Storage.Core.R2RML

[<ReferenceEquality>]
type SqlColumn = { Schema: SqlColumnSchema }

[<ReferenceEquality>]
type SqlSource = { Schema: ISqlTableSchema; Columns: SqlColumn list }

module SqlSource =
    let getColumn name source =
        source.Columns |> List.find (fun x -> x.Schema.Name = name)

type Literal =
    | String of string
    | Int of int
    | Double of double
    | DateTimeLiteral of DateTime

[<ReferenceEquality>]
type AssignedVariable = { SqlType: ISqlColumnType }

type Variable =
    | Assigned of AssignedVariable
    | Column of SqlColumn

type Condition =
    | AlwaysFalse
    | AlwaysTrue
    | Comparison of Comparisons * Expression * Expression
    | Conjunction of Condition list
    | Disjunction of Condition list
    | EqualVariableTo of Variable * Literal
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
    | IriSafeVariable of Variable
    | Constant of Literal
    | Concatenation of Expression list
    | Boolean of Condition
    | Null

type ExpressionSet = {
    IsNotErrorCondition: Condition
    TypeCategoryExpression: Expression
    TypeExpression: Expression
    StringExpression: Expression
    NumericExpression: Expression
    BooleanExpression: Expression
    DateTimeExpresion: Expression
}

module ExpressionSet =
    let empty =
        {
            IsNotErrorCondition=AlwaysFalse
            TypeCategoryExpression=Null
            TypeExpression=Null
            StringExpression=Null
            NumericExpression=Null
            BooleanExpression=Null
            DateTimeExpresion=Null
        }

type Assignment = { Variable: AssignedVariable; Expression: Expression }

type Ordering = { Expression: Expression; Direction: OrderingDirection }

type VariableSource =
    | Sql of SqlSource
    | SubQuery of CalculusModel
    | LeftOuterJoinModel of NotModifiedCalculusModel * Condition

and NotModifiedCalculusModel = { Sources: VariableSource list; Assignments: Assignment list; Filters: Condition list }

and ModifiedCalculusModel = { InnerModel: CalculusModel; Ordering: Ordering list; Limit: int option; Offset: int option; IsDistinct: bool }

and CalculusModel =
    | NoResult
    | SingleEmptyResult
    | Modified of ModifiedCalculusModel
    | Union of AssignedVariable * NotModifiedCalculusModel list
    | NotModified of NotModifiedCalculusModel

type ValueBinder =
    | EmptyValueBinder
    | BaseValueBinder of ObjectMapping * Map<string, Variable>
    | CoalesceValueBinder of ValueBinder list
    | CaseValueBinder of Variable * Map<int, ValueBinder>
    | ExpressionValueBinder of ExpressionSet
    | ConditionedValueBinder of Condition * ValueBinder

type BoundCalculusModel = { Model: CalculusModel; Bindings: Map<SparqlVariable, ValueBinder>; Variables: SparqlVariable list }
