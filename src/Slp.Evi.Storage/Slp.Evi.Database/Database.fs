namespace Slp.Evi.Database

open Slp.Evi.Common.Database
open System
open Slp.Evi.Relational.Algebra
open Slp.Evi.Common.Algebra

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
        abstract member WriteBinaryNumericOperation: operator:ArithmeticOperator * leftOperand:Expression * rightOperand:Expression -> unit
        abstract member WriteSwitch: caseStatements:CaseStatement list -> unit
        abstract member WriteCoalesce: expressions:Expression list -> unit
        abstract member WriteVariable: variable:Variable -> unit
        abstract member WriteIriSafeVariable: variable:Variable -> unit
        abstract member WriteConstant: literal:Literal -> unit
        abstract member WriteConcatenation: expressions:Expression list -> unit
        abstract member WriteBooleanExpression: condition:Condition -> unit

        abstract member WriteTrue: unit -> unit
        abstract member WriteFalse: unit -> unit
        abstract member WriteComparison: comparison:Comparisons * leftOperand:Expression * rightOperand:Expression -> unit
        abstract member WriteConjunction: conditions:Condition list -> unit
        abstract member WriteDisjunction: conditions:Condition list -> unit
        abstract member WriteEqualVariableTo: variable:Variable * literal:Literal -> unit
        abstract member WriteEqualVariables: leftVariable:Variable * rightVariable:Variable -> unit
        abstract member WriteIsNull: variable:Variable -> unit
        abstract member WriteLanguageMatch: langExpression:Expression * langRangeExpression:Expression -> unit
        abstract member WriteLikeMatch: expression:Expression * pattern:string -> unit
        abstract member WriteNot: condition:Condition -> unit

    let ProcessExpression (writer: ISqlExpressionWriter, expression: Expression) =
        match expression with
        | BinaryNumericOperation(operator, leftOperand, rightOperand) -> writer.WriteBinaryNumericOperation(operator, leftOperand, rightOperand)
        | Switch(caseStatements) -> writer.WriteSwitch(caseStatements)
        | Coalesce(expressions) -> writer.WriteCoalesce(expressions)
        | Variable(variable) -> writer.WriteVariable(variable)
        | IriSafeVariable(iriSafeVariable) -> writer.WriteIriSafeVariable(iriSafeVariable)
        | Constant(literal) -> writer.WriteConstant(literal)
        | Concatenation(expressions) -> writer.WriteConcatenation(expressions)
        | Boolean(condition) -> writer.WriteBooleanExpression(condition)
        | Null -> writer.WriteNull()

    let ProcessCondition (writer: ISqlExpressionWriter, condition: Condition) =
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
