using System.Linq;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    /// <summary>
    /// Static evaluation of the expressions
    /// </summary>
    public static class StaticExpressionEvaluationExtension
    {
        private static readonly StaticExpressionEvaluationVisitor Visitor = new StaticExpressionEvaluationVisitor();

        /// <summary>
        /// Evaluates the coalesceExpr.
        /// </summary>
        /// <param name="expression">The coalesceExpr.</param>
        /// <param name="data">The data.</param>
        /// <returns>The evaluated value.</returns>
        public static object StaticEvaluation(this IExpression expression, IQueryResultRow data)
        {
            return expression.Accept(Visitor, data);
        }

        /// <summary>
        /// Evaluates the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The data.</param>
        /// <returns>The evaluated value.</returns>
        public static bool StaticEvaluation(this ICondition condition, IQueryResultRow data)
        {
            return (bool)condition.Accept(Visitor, data);
        }

        /// <summary>
        /// Visitor to evaluate
        /// </summary>
        private class StaticExpressionEvaluationVisitor : IExpressionVisitor, IConditionVisitor
        {
            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(ColumnExpr expression, object data)
            {
                var rData = (IQueryResultRow)data;
                var col = rData.GetColumn(expression.Column.Name);

                // TODO: Iri escape
                return col.Value;
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(ConstantExpr expression, object data)
            {
                return expression.Value;
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(ConcatenationExpr expression, object data)
            {
                var parts = expression.Parts.Select(x => x.Accept(this, data).ToString());
                return string.Join(string.Empty, parts);
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="nullExpr">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(NullExpr nullExpr, object data)
            {
                return null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AlwaysFalseCondition condition, object data)
            {
                return false;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AlwaysTrueCondition condition, object data)
            {
                return true;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AndCondition condition, object data)
            {
                foreach (var inner in condition.Conditions)
                {
                    if (!(bool)inner.Accept(this, data))
                        return false;
                }

                return true;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(EqualsCondition condition, object data)
            {
                var leftExpr = condition.LeftOperand.Accept(this, data);
                var rightExpr = condition.RightOperand.Accept(this, data);

                return leftExpr.Equals(rightExpr);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(IsNullCondition condition, object data)
            {
                var rData = (IQueryResultRow)data;
                var col = rData.GetColumn(condition.Column.Name);

                return col.Value == null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(NotCondition condition, object data)
            {
                return !(bool)condition.InnerCondition.Accept(this, data);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(OrCondition condition, object data)
            {
                foreach (var inner in condition.Conditions)
                {
                    if ((bool)inner.Accept(this, data))
                        return true;
                }

                return false;
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="coalesceExpr">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(CoalesceExpr coalesceExpr, object data)
            {
                foreach (var expr in coalesceExpr.Expressions)
                {
                    var res = expr.Accept(this, data);

                    if (res != null)
                        return res;
                }

                return null;
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="caseExpr">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(CaseExpr caseExpr, object data)
            {
                foreach (var statement in caseExpr.Statements)
                {
                    if ((bool)statement.Condition.Accept(this, data))
                        return statement.Expression.Accept(this, data);
                }

                return null;
            }
        }
    }
}
