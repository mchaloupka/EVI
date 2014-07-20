using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    /// <summary>
    /// Extension for replacing column reference
    /// </summary>
    public static class ReplaceColumnReferenceExtension
    {
        /// <summary>
        /// Replaces the column reference.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public static void ReplaceColumnReference(this ICondition condition, ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            var visitor = new ReplaceColumnReferenceVisitor(oldColumn, newColumn);
            condition.Accept(visitor, null);
        }

        /// <summary>
        /// Replaces the column reference.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public static void ReplaceColumnReference(this IExpression expression, ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            var visitor = new ReplaceColumnReferenceVisitor(oldColumn, newColumn);
            expression.Accept(visitor, null);
        }

        /// <summary>
        /// Visitor for replacing column reference
        /// </summary>
        private class ReplaceColumnReferenceVisitor : IConditionVisitor, IExpressionVisitor
        {
            /// <summary>
            /// The old column
            /// </summary>
            private ISqlColumn oldColumn;

            /// <summary>
            /// The new column
            /// </summary>
            private ISqlColumn newColumn;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReplaceColumnReferenceVisitor"/> class.
            /// </summary>
            /// <param name="oldColumn">The old column.</param>
            /// <param name="newColumn">The new column.</param>
            public ReplaceColumnReferenceVisitor(ISqlColumn oldColumn, ISqlColumn newColumn)
            {
                this.oldColumn = oldColumn;
                this.newColumn = newColumn;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AlwaysFalseCondition condition, object data)
            {
                return null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AlwaysTrueCondition condition, object data)
            {
                return null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AndCondition condition, object data)
            {
                foreach (var item in condition.Conditions)
                {
                    item.Accept(this, data);
                }

                return null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(EqualsCondition condition, object data)
            {
                condition.LeftOperand.Accept(this, data);
                condition.RightOperand.Accept(this, data);
                return null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(IsNullCondition condition, object data)
            {
                if (condition.Column == this.oldColumn)
                    condition.Column = this.newColumn;

                return null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(NotCondition condition, object data)
            {
                condition.InnerCondition.Accept(this, data);
                return null;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(OrCondition condition, object data)
            {
                foreach (var item in condition.Conditions)
                {
                    item.Accept(this, data);
                }

                return null;
            }

            /// <summary>
            /// Visits the specified expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(ColumnExpr expression, object data)
            {
                if (expression.Column == oldColumn)
                    expression.Column = newColumn;

                return null;
            }

            /// <summary>
            /// Visits the specified expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(ConstantExpr expression, object data)
            {
                return null;
            }

            /// <summary>
            /// Visits the specified expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(ConcatenationExpr expression, object data)
            {
                foreach (var item in expression.Parts)
                {
                    item.Accept(this, data);
                }

                return null;
            }

            /// <summary>
            /// Visits the specified expression.
            /// </summary>
            /// <param name="nullExpr">The expression.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(NullExpr nullExpr, object data)
            {
                return null;
            }


            /// <summary>
            /// Visits the specified expression.
            /// </summary>
            /// <param name="collateExpr">The expression.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(CoalesceExpr collateExpr, object data)
            {
                foreach (var expr in collateExpr.Expressions)
                {
                    expr.Accept(this, data);
                }

                return null;
            }

            /// <summary>
            /// Visits the specified expression.
            /// </summary>
            /// <param name="caseExpr">The expression.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(CaseExpr caseExpr, object data)
            {
                foreach (var statement in caseExpr.Statements)
                {
                    statement.Condition.Accept(this, data);
                    statement.Expression.Accept(this, data);
                }

                return null;
            }
        }
    }
}
