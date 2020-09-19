namespace Slp.Evi.Database

open System
open Slp.Evi.Common.Algebra
open Slp.Evi.Relational.Algebra
open Slp.Evi.Common.Database
open Slp.Evi.Database

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

    let private writeExpression (writer: ISqlExpressionWriter, expressionContent: TypedExpressionContent) =
        match expressionContent with
        | BinaryNumericOperation(operator, leftOperand, rightOperand) -> writer.WriteBinaryNumericOperation(operator, leftOperand, rightOperand)
        | Switch(caseStatements) -> writer.WriteSwitch(caseStatements)
        | Coalesce(expressions) -> writer.WriteCoalesce(expressions)
        | Variable(variable) -> writer.WriteVariable(variable)
        | IriSafeVariable(iriSafeVariable) -> writer.WriteIriSafeVariable(iriSafeVariable)
        | Constant(literal) ->
            match literal with
            | String(s) -> writer.WriteConstant(s)
            | Double(d) -> writer.WriteConstant(d)
            | Int(i) -> writer.WriteConstant(i)
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