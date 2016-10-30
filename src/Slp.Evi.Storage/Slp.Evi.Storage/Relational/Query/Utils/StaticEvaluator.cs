using System;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Storage.Relational.Query.Utils
{
    /// <summary>
    /// Provides the ability to evaluate expressions and conditions
    /// </summary>
    public class StaticEvaluator
        : IExpressionVisitor, IFilterConditionVisitor
    {
        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        public object Evaluate(IExpression expression, IQueryResultRow rowData, QueryContext context)
        {
            return expression.Accept(this, new StaticEvaluatorParameter(rowData, context));
        }

        /// <summary>
        /// Helper class representing passed parameter
        /// </summary>
        private class StaticEvaluatorParameter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StaticEvaluatorParameter"/> class.
            /// </summary>
            /// <param name="rowData">The row data.</param>
            /// <param name="context">The context.</param>
            public StaticEvaluatorParameter(IQueryResultRow rowData, QueryContext context)
            {
                RowData = rowData;
                Context = context;
            }

            /// <summary>
            /// Gets the row data.
            /// </summary>
            /// <value>The row data.</value>
            public IQueryResultRow RowData { get; }

            /// <summary>
            /// Gets the context.
            /// </summary>
            /// <value>The context.</value>
            public QueryContext Context { get; }
        }

        /// <summary>
        /// Visits <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="columnExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IExpressionVisitor.Visit(ColumnExpression columnExpression, object data)
        {
            // TODO: Iri escaping
            var param = (StaticEvaluatorParameter) data;
            var columnName = param.Context.QueryNamingHelpers.GetVariableName(null, columnExpression.CalculusVariable);
            var column = param.RowData.GetColumn(columnName);
            return column.StringValue;
        }

        /// <summary>
        /// Visits <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="concatenationExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IExpressionVisitor.Visit(ConcatenationExpression concatenationExpression, object data)
        {
            var inners = concatenationExpression.InnerExpressions.Select(x => x.Accept(this, data)).ToArray();

            if (inners.Any(x => x == null))
            {
                return null;
            }
            else
            {
                return string.Join(string.Empty, inners);
            }
        }

        /// <summary>
        /// Visits <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="constantExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IExpressionVisitor.Visit(ConstantExpression constantExpression, object data)
        {
            return constantExpression.Value;
        }

        /// <summary>
        /// Visits <see cref="CaseExpression"/>
        /// </summary>
        /// <param name="caseExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IExpressionVisitor.Visit(CaseExpression caseExpression, object data)
        {
            foreach (var statement in caseExpression.Statements)
            {
                if ((bool) statement.Condition.Accept(this, data))
                {
                    return statement.Expression.Accept(this, data);
                }
            }

            return null;
        }

        /// <summary>
        /// Visits <see cref="CoalesceExpression"/>
        /// </summary>
        /// <param name="coalesceExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IExpressionVisitor.Visit(CoalesceExpression coalesceExpression, object data)
        {
            foreach (var expression in coalesceExpression.InnerExpressions)
            {
                var innerResult = expression.Accept(this, data);

                if (innerResult != null)
                {
                    return innerResult;
                }
            }

            return null;
        }

        /// <summary>
        /// Visits <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="alwaysFalseCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(AlwaysFalseCondition alwaysFalseCondition, object data)
        {
            return false;
        }

        /// <summary>
        /// Visits <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="alwaysTrueCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(AlwaysTrueCondition alwaysTrueCondition, object data)
        {
            return true;
        }

        /// <summary>
        /// Visits <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="conjunctionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(ConjunctionCondition conjunctionCondition, object data)
        {
            foreach (var filterCondition in conjunctionCondition.InnerConditions)
            {
                if (!(bool) filterCondition.Accept(this, data))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Visits <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="disjunctionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(DisjunctionCondition disjunctionCondition, object data)
        {
            foreach (var filterCondition in disjunctionCondition.InnerConditions)
            {
                if ((bool)filterCondition.Accept(this, data))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Visits <see cref="ComparisonCondition"/>
        /// </summary>
        /// <param name="comparisonCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(ComparisonCondition comparisonCondition, object data)
        {
            var left = (IComparable)comparisonCondition.LeftOperand.Accept(this, data);
            var right = (IComparable)comparisonCondition.RightOperand.Accept(this, data);

            var comparison = left.CompareTo(right);

            switch (comparisonCondition.ComparisonType)
            {
                case ComparisonTypes.GreaterThan:
                    return comparison > 0;
                case ComparisonTypes.GreaterOrEqualThan:
                    return comparison >= 0;
                case ComparisonTypes.LessThan:
                    return comparison < 0;
                case ComparisonTypes.LessOrEqualThan:
                    return comparison <= 0;
                case ComparisonTypes.EqualTo:
                    return comparison == 0;
                case ComparisonTypes.NotEqualTo:
                    return comparison != 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Visits <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="equalVariablesCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(EqualVariablesCondition equalVariablesCondition, object data)
        {
            var param = (StaticEvaluatorParameter)data;
            var leftColumnName = param.Context.QueryNamingHelpers.GetVariableName(null, equalVariablesCondition.LeftVariable);
            var leftColumn = param.RowData.GetColumn(leftColumnName);
            var rightColumnName = param.Context.QueryNamingHelpers.GetVariableName(null, equalVariablesCondition.RightVariable);
            var rightColumn = param.RowData.GetColumn(rightColumnName);

            return leftColumn.StringValue.Equals(rightColumn.StringValue);
        }

        /// <summary>
        /// Visits <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="isNullCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(IsNullCondition isNullCondition, object data)
        {
            var param = (StaticEvaluatorParameter)data;
            var columnName = param.Context.QueryNamingHelpers.GetVariableName(null, isNullCondition.Variable);
            var column = param.RowData.GetColumn(columnName);
            return column.StringValue == null;
        }

        /// <summary>
        /// Visits <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="negationCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object IFilterConditionVisitor.Visit(NegationCondition negationCondition, object data)
        {
            var inner = (bool)negationCondition.InnerCondition.Accept(this, data);
            return !inner;
        }
    }
}
