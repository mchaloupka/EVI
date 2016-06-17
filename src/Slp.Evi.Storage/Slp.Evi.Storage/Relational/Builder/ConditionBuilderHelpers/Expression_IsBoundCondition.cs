using System;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// Helper for <see cref="ConditionBuilder.CreateIsBoundCondition(IExpression, QueryContext)"/>.
    /// </summary>
    public class Expression_IsBoundCondition
        : IExpressionVisitor
    {
        /// <summary>
        /// The condition builder
        /// </summary>
        private readonly ConditionBuilder _conditionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression_IsBoundCondition"/> class.
        /// </summary>
        /// <param name="conditionBuilder">The condition builder.</param>
        public Expression_IsBoundCondition(ConditionBuilder conditionBuilder)
        {
            _conditionBuilder = conditionBuilder;
        }

        /// <summary>
        /// Creates the is bound condition.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateIsBoundCondition(IExpression expression, QueryContext context)
        {
            return (IFilterCondition) expression.Accept(this, context);
        }

        /// <summary>
        /// Visits <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="columnExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ColumnExpression columnExpression, object data)
        {
            return new NegationCondition(new IsNullCondition(columnExpression.CalculusVariable));
        }

        /// <summary>
        /// Visits <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="concatenationExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConcatenationExpression concatenationExpression, object data)
        {
            return
                new ConjunctionCondition(
                    concatenationExpression.InnerExpressions.Select(x => (IFilterCondition) x.Accept(this, data))
                        .ToList());
        }

        /// <summary>
        /// Visits <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="constantExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConstantExpression constantExpression, object data)
        {
            return new AlwaysTrueCondition();
        }

        /// <summary>
        /// Visits <see cref="CaseExpression"/>
        /// </summary>
        /// <param name="caseExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CaseExpression caseExpression, object data)
        {
            return
                new DisjunctionCondition(
                    caseExpression.Statements.Select(s => 
                    new ConjunctionCondition(new IFilterCondition[]
                    {
                        s.Condition,
                        (IFilterCondition)s.Expression.Accept(this, data)
                    })));
        }

        /// <summary>
        /// Visits <see cref="CoalesceExpression"/>
        /// </summary>
        /// <param name="coalesceExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CoalesceExpression coalesceExpression, object data)
        {
            return
                new DisjunctionCondition(
                    coalesceExpression.InnerExpressions.Select(x => (IFilterCondition) x.Accept(this, data)).ToList());
        }
    }
}