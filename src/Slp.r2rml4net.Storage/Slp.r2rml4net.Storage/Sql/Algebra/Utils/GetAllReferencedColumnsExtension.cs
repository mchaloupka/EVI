using System.Collections.Generic;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    /// <summary>
    /// Extension for getting all referenced columns
    /// </summary>
    public static class GetAllReferencedColumnsExtension
    {
        /// <summary>
        /// The visitor
        /// </summary>
        private static readonly GetAllReferencedColumnsVisitor Visitor = new GetAllReferencedColumnsVisitor();

        /// <summary>
        /// Gets all referenced columns.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>The referenced columns.</returns>
        public static IEnumerable<ISqlColumn> GetAllReferencedColumns(this ICondition condition)
        {
            return (IEnumerable<ISqlColumn>)condition.Accept(Visitor, null);
        }

        /// <summary>
        /// Gets all referenced columns.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The referenced columns.</returns>
        public static IEnumerable<ISqlColumn> GetAllReferencedColumns(this IExpression expression)
        {
            return (IEnumerable<ISqlColumn>)expression.Accept(Visitor, null);
        }

        /// <summary>
        /// The visitor that gets all referenced columns
        /// </summary>
        private class GetAllReferencedColumnsVisitor : IConditionVisitor, IExpressionVisitor
        {
            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(AlwaysFalseCondition condition, object data)
            {
                yield break;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(AlwaysTrueCondition condition, object data)
            {
                yield break;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(AndCondition condition, object data)
            {
                foreach (var subCond in condition.Conditions)
                {
                    var res = (IEnumerable<ISqlColumn>)subCond.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(EqualsCondition condition, object data)
            {
                var leftRes = (IEnumerable<ISqlColumn>)condition.LeftOperand.Accept(this, data);
                var rightRes = (IEnumerable<ISqlColumn>)condition.RightOperand.Accept(this, data);

                foreach (var item in leftRes)
                {
                    yield return item;
                }

                foreach (var item in rightRes)
                {
                    yield return item;
                }
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(IsNullCondition condition, object data)
            {
                yield return condition.Column;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(NotCondition condition, object data)
            {
                return (IEnumerable<ISqlColumn>)condition.InnerCondition.Accept(this, data);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(OrCondition condition, object data)
            {
                foreach (var subCond in condition.Conditions)
                {
                    var res = (IEnumerable<ISqlColumn>)subCond.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(ColumnExpr expression, object data)
            {
                yield return expression.Column;
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(ConstantExpr expression, object data)
            {
                yield break;
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(ConcatenationExpr expression, object data)
            {
                foreach (var subExpr in expression.Parts)
                {
                    var res = (IEnumerable<ISqlColumn>)subExpr.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="collateExpr">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(CoalesceExpr collateExpr, object data)
            {
                foreach (var subExpr in collateExpr.Expressions)
                {
                    var res = (IEnumerable<ISqlColumn>)subExpr.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="caseExpr">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(CaseExpr caseExpr, object data)
            {
                foreach (var subExpr in caseExpr.Statements)
                {
                    var res = (IEnumerable<ISqlColumn>)subExpr.Condition.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }

                    res = (IEnumerable<ISqlColumn>)subExpr.Expression.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="nullExpr">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>The referenced columns.</returns>
            public IEnumerable<ISqlColumn> Visit(NullExpr nullExpr, object data)
            {
                yield break;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IConditionVisitor.Visit(AlwaysFalseCondition condition, object data)
            {
                return Visit(condition, data);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IConditionVisitor.Visit(AlwaysTrueCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IConditionVisitor.Visit(AndCondition condition, object data)
            {
                return Visit(condition, data);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IConditionVisitor.Visit(EqualsCondition condition, object data)
            {
                return Visit(condition, data);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IConditionVisitor.Visit(IsNullCondition condition, object data)
            {
                return Visit(condition, data);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IConditionVisitor.Visit(OrCondition condition, object data)
            {
                return Visit(condition, data);
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IConditionVisitor.Visit(NotCondition condition, object data)
            {
                return Visit(condition, data);
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IExpressionVisitor.Visit(ColumnExpr expression, object data)
            {
                return Visit(expression, data);
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IExpressionVisitor.Visit(ConstantExpr expression, object data)
            {
                return Visit(expression, data);
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            object IExpressionVisitor.Visit(ConcatenationExpr expression, object data)
            {
                return Visit(expression, data);
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The data.</param>
            /// <returns>System.Object.</returns>
            object IExpressionVisitor.Visit(NullExpr expression, object data)
            {
                return Visit(expression, data);
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="coalesceExpr">The coalesceExpr.</param>
            /// <param name="data">The data.</param>
            /// <returns>System.Object.</returns>
            object IExpressionVisitor.Visit(CoalesceExpr coalesceExpr, object data)
            {
                return Visit(coalesceExpr, data);
            }

            /// <summary>
            /// Visits the specified coalesceExpr.
            /// </summary>
            /// <param name="expression">The coalesceExpr.</param>
            /// <param name="data">The data.</param>
            /// <returns>System.Object.</returns>
            object IExpressionVisitor.Visit(CaseExpr expression, object data)
            {
                return Visit(expression, data);
            }
        }
    }
}
