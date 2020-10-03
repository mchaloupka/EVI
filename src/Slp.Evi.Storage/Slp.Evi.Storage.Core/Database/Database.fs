namespace Slp.Evi.Storage.Core.Database

open System
open Slp.Evi.Storage.Core.Common.Algebra
open Slp.Evi.Storage.Core.Relational.Algebra
open Slp.Evi.Storage.Core.Common.Database
open Slp.Evi.Storage.Core.Database

type VariableValue =
    | IntVariableValue of int
    | BooleanVariableValue of bool
    | StringVariableValue of string
    | DoubleVariableValue of double
    | DateTimeVariableValue of DateTime
    | NullVariableValue

module VariableValue =
    [<CompiledName("AsBoolean")>]
    let asBoolean = function
        | BooleanVariableValue b -> b
        | x -> sprintf "Cannot interpret value as boolean: %A" x |> invalidOp

    [<CompiledName("AsInteger")>]
    let asInteger = function
        | IntVariableValue i -> i
        | x -> sprintf "Cannot interpret value as int: %A" x |> invalidOp

    [<CompiledName("AsString")>]
    let asString = function
        | StringVariableValue s -> s
        | IntVariableValue i -> System.Xml.XmlConvert.ToString i
        | DoubleVariableValue d -> System.Xml.XmlConvert.ToString d
        | BooleanVariableValue b -> System.Xml.XmlConvert.ToString b
        | DateTimeVariableValue d -> System.Xml.XmlConvert.ToString (d, System.Xml.XmlDateTimeSerializationMode.Utc);
        | NullVariableValue ->
            "Cannot interpret null value as string" |> invalidOp

    [<CompiledName("TryAsString")>]
    let tryAsString = function
        | NullVariableValue -> None
        | x ->
            x |> asString |> Some

type ISqlResultColumn =
    abstract member Name: string with get

    abstract member VariableValue: VariableValue with get

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

module SqlDatabaseWriterHelper =
    type ISqlExpressionWriter =
        abstract member WriteNull: unit -> unit
        abstract member WriteBinaryNumericOperation: operator:ArithmeticOperator * leftOperand: TypedExpression * rightOperand: TypedExpression -> unit
        abstract member WriteSwitch: caseStatements: TypedCaseStatement list -> unit
        abstract member WriteCoalesce: expressions: TypedExpression list -> unit
        abstract member WriteVariable: variable: Variable -> unit
        abstract member WriteIriSafeVariable: variable: Variable -> unit
        abstract member WriteConcatenation: expressions: TypedExpression list -> unit
        abstract member WriteBooleanExpression: condition: TypedCondition -> unit
        abstract member WriteConstant: literal:string -> unit
        abstract member WriteConstant: literal:double -> unit
        abstract member WriteConstant: literal:int -> unit
        abstract member WriteConstant: literal:DateTime -> unit

        abstract member WriteTrue: unit -> unit
        abstract member WriteFalse: unit -> unit
        abstract member WriteComparison: comparison:Comparisons * leftOperand: TypedExpression * rightOperand: TypedExpression -> unit
        abstract member WriteConjunction: conditions: TypedCondition list -> unit
        abstract member WriteDisjunction: conditions: TypedCondition list -> unit
        abstract member WriteEqualVariableTo: variable: Variable * literal: Literal -> unit
        abstract member WriteEqualVariables: leftVariable: Variable * rightVariable: Variable -> unit
        abstract member WriteIsNull: variable: Variable -> unit
        abstract member WriteLanguageMatch: langExpression: TypedExpression * langRangeExpression:TypedExpression -> unit
        abstract member WriteLikeMatch: expression: TypedExpression * pattern: string -> unit
        abstract member WriteNot: condition: TypedCondition -> unit

        abstract member WriteCastedExpression: actualType: ISqlColumnType * expectedType: ISqlColumnType * writeExpressionFunc: Action -> unit

    [<CompiledName("ProcessLiteral")>]
    let processLiteral (writer: ISqlExpressionWriter, literal: Literal) =
        match literal with
        | String(s) -> writer.WriteConstant(s)
        | Double(d) -> writer.WriteConstant(d)
        | Int(i) -> writer.WriteConstant(i)
        | DateTimeLiteral(d) -> writer.WriteConstant(d)

    let private writeExpression (writer: ISqlExpressionWriter, expressionContent: TypedExpressionContent) =
        match expressionContent with
        | BinaryNumericOperation(operator, leftOperand, rightOperand) -> writer.WriteBinaryNumericOperation(operator, leftOperand, rightOperand)
        | Switch(caseStatements) -> writer.WriteSwitch(caseStatements)
        | Coalesce(expressions) -> writer.WriteCoalesce(expressions)
        | Variable(variable) -> writer.WriteVariable(variable)
        | IriSafeVariable(iriSafeVariable) -> writer.WriteIriSafeVariable(iriSafeVariable)
        | Constant(literal) ->
            processLiteral (writer, literal)
        | Concatenation(expressions) -> writer.WriteConcatenation(expressions)
        | Boolean(condition) -> writer.WriteBooleanExpression(condition)
        | Null -> writer.WriteNull()

    [<CompiledName("ProcessExpression")>]
    let processExpression (writer: ISqlExpressionWriter, expression: TypedExpression) =
        let writeExpressionFunc = fun () -> writeExpression(writer, expression.Expression)
        writer.WriteCastedExpression(expression.ActualType, expression.ProvidedType, new Action(writeExpressionFunc))

    [<CompiledName("ProcessCondition")>]
    let processCondition (writer: ISqlExpressionWriter, condition: TypedCondition) =
        match condition with
        | AlwaysFalse -> writer.WriteFalse()
        | AlwaysTrue -> writer.WriteTrue()
        | Comparison(comparison, leftOperand, rightOperand) -> writer.WriteComparison(comparison, leftOperand, rightOperand)
        | Conjunction(conditions) -> writer.WriteConjunction(conditions)
        | Disjunction(conditions) -> writer.WriteDisjunction(conditions)
        | EqualVariableTo(variable, literal) -> writer.WriteEqualVariableTo(variable, literal)
        | EqualVariables(leftVariable, rightVariable) -> writer.WriteEqualVariables(leftVariable, rightVariable)
        | IsNull(variable) -> writer.WriteIsNull(variable)
        | LanguageMatch(langExpression, langRangeExpression) -> writer.WriteLanguageMatch(langExpression, langRangeExpression)
        | Like(expression, pattern) -> writer.WriteLikeMatch(expression, pattern)
        | Not(condition) -> writer.WriteNot(condition)