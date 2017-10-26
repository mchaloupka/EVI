using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Types;

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
        public ExpressionsSet CreateExpression(ISparqlExpression expression, IQueryContext context, List<IValueBinder> valueBinders)
        {
            var parameter = new ExpressionVisitParameter(context, valueBinders);
            return CreateExpression(expression, parameter);
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameter">The parameter.</param>
        public ExpressionsSet CreateExpression(ISparqlExpression expression, ExpressionVisitParameter parameter)
        {
            return (ExpressionsSet) expression.Accept(this, parameter);
        }

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="context">The context.</param>
        /// <param name="valueBinders">The value binders.</param>
        public ConditionPart CreateCondition(ISparqlCondition condition, IQueryContext context, IEnumerable<IValueBinder> valueBinders)
        {
            var parameter = new ExpressionVisitParameter(context, valueBinders);
            return CreateCondition(condition, parameter);
        }

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="parameter">The parameter.</param>
        public ConditionPart CreateCondition(ISparqlCondition condition, ExpressionVisitParameter parameter)
        {
            return (ConditionPart)condition.Accept(this, parameter);
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
            var inner = CreateCondition(negationExpression.InnerCondition, (ExpressionVisitParameter)data);

            return new ConditionPart(inner.IsNotErrorCondition, new NegationCondition(inner.MainCondition));
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
                conjunctionExpression.Operands
                .Select(x => CreateCondition(x, (ExpressionVisitParameter) data))
                .ToList();

            return new ConditionPart(
                new ConjunctionCondition(innerConditions.Select(x => x.IsNotErrorCondition)),
                new ConjunctionCondition(innerConditions.Select(x => x.MainCondition)));
        }

        /// <summary>
        /// Visits <see cref="ComparisonExpression"/>
        /// </summary>
        /// <param name="comparisonExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ComparisonExpression comparisonExpression, object data)
        {
            var parameter = (ExpressionVisitParameter) data;
            var left = CreateExpression(comparisonExpression.LeftOperand, parameter);
            var right = CreateExpression(comparisonExpression.RightOperand, parameter);

            List<IFilterCondition> conditions = new List<IFilterCondition>();
            List<IFilterCondition> notAnErrorConditions = new List<IFilterCondition>();

            notAnErrorConditions.Add(
                new ComparisonCondition(left.TypeCategoryExpression, right.TypeCategoryExpression,
                    ComparisonTypes.EqualTo));

            switch (comparisonExpression.ComparisonType)
            {
                case ComparisonTypes.GreaterThan:
                case ComparisonTypes.GreaterOrEqualThan:
                case ComparisonTypes.LessThan:
                case ComparisonTypes.LessOrEqualThan:
                    notAnErrorConditions.Add(new ComparisonCondition(left.TypeCategoryExpression, right.TypeCategoryExpression, ComparisonTypes.EqualTo));
                    notAnErrorConditions.Add(new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int)TypeCategories.BlankNode, parameter.QueryContext), ComparisonTypes.NotEqualTo));
                    notAnErrorConditions.Add(new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int)TypeCategories.SimpleLiteral, parameter.QueryContext), ComparisonTypes.NotEqualTo));
                    notAnErrorConditions.Add(new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int)TypeCategories.OtherLiterals, parameter.QueryContext), ComparisonTypes.NotEqualTo));
                    break;
            }

            // Comparison of simple literal
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.SimpleLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(right.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.SimpleLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(left.StringExpression, right.StringExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of string literal
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.StringLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(right.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.StringLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(left.StringExpression, right.StringExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of numeric literal
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.NumericLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(right.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.NumericLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(left.NumericExpression, right.NumericExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of boolean literal
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.BooleanLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(right.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.BooleanLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(left.BooleanExpression, right.BooleanExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of datetime literal
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.DateTimeLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(right.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.DateTimeLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(left.DateTimeExpression, right.DateTimeExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of all other literals
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression, right.TypeCategoryExpression,
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(left.TypeExpression, right.TypeExpression,
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(left.StringExpression, right.StringExpression,
                    comparisonExpression.ComparisonType),
            }));

            var mainCondition = new DisjunctionCondition(conditions);
            var noErrorCondition = new ConjunctionCondition(notAnErrorConditions);

            return new ConditionPart(noErrorCondition, mainCondition);
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
                disjunctionExpression.Operands
                .Select(x => CreateCondition(x, (ExpressionVisitParameter) data))
                    .ToList();

            return new ConditionPart(
                new ConjunctionCondition(innerConditions.Select(x => x.IsNotErrorCondition)),
                new DisjunctionCondition(innerConditions.Select(x => x.MainCondition))
                );
        }
    }
}