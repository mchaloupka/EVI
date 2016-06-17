using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// Helper for <see cref="ConditionBuilder.CreateCondition"/>.
    /// </summary>
    public class SparqlExpression_CreateExpression
        : ISparqlExpressionVisitor
    {
        /// <summary>
        /// The condition builder
        /// </summary>
        private readonly ConditionBuilder _conditionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparqlExpression_CreateExpression"/> class.
        /// </summary>
        /// <param name="conditionBuilder">The condition builder.</param>
        public SparqlExpression_CreateExpression(ConditionBuilder conditionBuilder)
        {
            _conditionBuilder = conditionBuilder;
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <returns>IExpression.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IExpression CreateExpression(ISparqlExpression expression, QueryContext context, List<IValueBinder> valueBinders)
        {
            var parameter = new ExpressionVisitParameter(context, valueBinders);
            return (IExpression)expression.Accept(this, parameter);
        }

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="context">The context.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <returns>IFilterCondition.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IFilterCondition CreateCondition(ISparqlCondition condition, QueryContext context, IEnumerable<IValueBinder> valueBinders)
        {
            var parameter = new ExpressionVisitParameter(context, valueBinders);
            return (IFilterCondition) condition.Accept(this, parameter);
        }

        /// <summary>
        /// Visits <see cref="IsBoundExpression"/>
        /// </summary>
        /// <param name="isBoundExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(IsBoundExpression isBoundExpression, object data)
        {
            var param = (ExpressionVisitParameter)data;

            IValueBinder valueBinder;
            if (param.ValueBinders.TryGetValue(isBoundExpression.Variable, out valueBinder))
            {
                return _conditionBuilder.CreateIsBoundCondition(valueBinder, param.QueryContext);
            }
            else
            {
                return new AlwaysFalseCondition();
            }
        }

        /// <summary>
        /// Visits <see cref="BooleanTrueExpression"/>
        /// </summary>
        /// <param name="booleanTrueExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BooleanTrueExpression booleanTrueExpression, object data)
        {
            return new AlwaysTrueCondition();
        }

        /// <summary>
        /// Visits <see cref="BooleanFalseExpression" />
        /// </summary>
        /// <param name="booleanFalseExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BooleanFalseExpression booleanFalseExpression, object data)
        {
            return new AlwaysFalseCondition();
        }

        /// <summary>
        /// Visits <see cref="NegationExpression"/>
        /// </summary>
        /// <param name="negationExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NegationExpression negationExpression, object data)
        {
            var inner = (IFilterCondition)negationExpression.InnerCondition.Accept(this, data);
            return new NegationCondition(inner);
        }

        /// <summary>
        /// Visits <see cref="VariableExpression"/>
        /// </summary>
        /// <param name="variableExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(VariableExpression variableExpression, object data)
        {
            var parameter = (ExpressionVisitParameter)data;
            var valueBinder = parameter.ValueBinders[variableExpression.Variable];
            return _conditionBuilder.CreateExpression(parameter.QueryContext, valueBinder);
        }

        /// <summary>
        /// Visits <see cref="ConjunctionExpression"/>
        /// </summary>
        /// <param name="conjunctionExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConjunctionExpression conjunctionExpression, object data)
        {
            var innerConditions =
                conjunctionExpression.Operands.Select(x => x.Accept(this, data)).OfType<IFilterCondition>();

            return new ConjunctionCondition(innerConditions);
        }

        /// <summary>
        /// Visits <see cref="ComparisonExpression"/>
        /// </summary>
        /// <param name="comparisonExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ComparisonExpression comparisonExpression, object data)
        {
            var left = (IExpression)comparisonExpression.LeftOperand.Accept(this, data);
            var right = (IExpression)comparisonExpression.RightOperand.Accept(this, data);
            return new ComparisonCondition(left, right, comparisonExpression.ComparisonType);
        }

        /// <summary>
        /// Visits <see cref="NodeExpression"/>
        /// </summary>
        /// <param name="nodeExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NodeExpression nodeExpression, object data)
        {
            var parameter = (ExpressionVisitParameter)data;
            return _conditionBuilder.CreateExpression(parameter.QueryContext, nodeExpression.Node);
        }

        /// <summary>
        /// Visits <see cref="DisjunctionExpression"/>
        /// </summary>
        /// <param name="disjunctionExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(DisjunctionExpression disjunctionExpression, object data)
        {
            var innerConditions =
                disjunctionExpression.Operands.Select(x => x.Accept(this, data)).OfType<IFilterCondition>();

            return new DisjunctionCondition(innerConditions);
        }
    }
}