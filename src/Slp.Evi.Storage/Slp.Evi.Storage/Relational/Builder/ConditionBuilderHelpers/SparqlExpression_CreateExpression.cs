using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Types;
using Slp.Evi.Storage.Utils;
using VDS.RDF.Parsing;

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
            IFilterCondition condition;

            IValueBinder valueBinder;
            if (param.ValueBinders.TryGetValue(isBoundExpression.Variable, out valueBinder))
            {
                condition = _conditionBuilder.CreateIsBoundCondition(valueBinder, param.QueryContext);
            }
            else
            {
                condition = new AlwaysFalseCondition();
            }

            return new ConditionPart(new AlwaysTrueCondition(), condition);
        }

        /// <summary>
        /// Visits <see cref="BooleanTrueExpression"/>
        /// </summary>
        /// <param name="booleanTrueExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BooleanTrueExpression booleanTrueExpression, object data)
        {
            return new ConditionPart(new AlwaysTrueCondition(), new AlwaysTrueCondition());
        }

        /// <summary>
        /// Visits <see cref="BooleanFalseExpression" />
        /// </summary>
        /// <param name="booleanFalseExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BooleanFalseExpression booleanFalseExpression, object data)
        {
            return new ConditionPart(new AlwaysTrueCondition(), new AlwaysFalseCondition());
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
                new ComparisonCondition(left.StringExpression, right.StringExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of string literal
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression,
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
                new ComparisonCondition(left.NumericExpression, right.NumericExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of boolean literal
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeCategoryExpression,
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
                new ComparisonCondition(left.DateTimeExpression, right.DateTimeExpression,
                    comparisonExpression.ComparisonType),
            }));

            // Comparison of all other literals
            conditions.Add(new ConjunctionCondition(new IFilterCondition[]
            {
                new ComparisonCondition(left.TypeExpression, right.TypeExpression,
                    ComparisonTypes.EqualTo),
                new DisjunctionCondition(new IFilterCondition[]
                {
                    new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int) TypeCategories.BlankNode, parameter.QueryContext), ComparisonTypes.EqualTo),
                    new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int) TypeCategories.IRI, parameter.QueryContext), ComparisonTypes.EqualTo),
                    new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int) TypeCategories.SimpleLiteral, parameter.QueryContext), ComparisonTypes.EqualTo),
                    new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int) TypeCategories.StringLiteral, parameter.QueryContext), ComparisonTypes.EqualTo),
                    new ComparisonCondition(left.TypeCategoryExpression, new ConstantExpression((int) TypeCategories.OtherLiterals, parameter.QueryContext), ComparisonTypes.EqualTo)
                }),
                new ComparisonCondition(left.StringExpression, right.StringExpression,
                    comparisonExpression.ComparisonType)
            }));

            var mainCondition = new DisjunctionCondition(conditions);
            IFilterCondition noErrorCondition;

            if (notAnErrorConditions.Count == 0)
            {
                noErrorCondition = new AlwaysTrueCondition();
            }
            else if (notAnErrorConditions.Count == 1)
            {
                noErrorCondition = notAnErrorConditions[0];
            }
            else
            {
                noErrorCondition = new ConjunctionCondition(notAnErrorConditions);
            }

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

        /// <inheritdoc />
        public object Visit(BinaryArithmeticExpression binaryArithmeticExpression, object data)
        {
            var parameter = (ExpressionVisitParameter) data;
            var leftExpression = CreateExpression(binaryArithmeticExpression.LeftOperand, parameter);
            var rightExpression = CreateExpression(binaryArithmeticExpression.RightOperand, parameter);

            var notErrorCondition = new ConjunctionCondition(new[]
            {
                leftExpression.IsNotErrorCondition,
                rightExpression.IsNotErrorCondition,
                new ComparisonCondition(leftExpression.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.NumericLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo),
                new ComparisonCondition(rightExpression.TypeCategoryExpression,
                    new ConstantExpression((int) TypeCategories.NumericLiteral, parameter.QueryContext),
                    ComparisonTypes.EqualTo)
            });

            var numericExpression = new BinaryNumericExpression(leftExpression.NumericExpression, rightExpression.NumericExpression,
                binaryArithmeticExpression.Operation, parameter.QueryContext);

            var numericType = parameter.QueryContext.TypeCache.GetValueTypeForDataType(EviConstants.XsdDecimal);

            return new ExpressionsSet(
                notErrorCondition,
                new ConstantExpression(parameter.QueryContext.TypeCache.GetIndex(numericType), parameter.QueryContext),
                new ConstantExpression((int) numericType.Category, parameter.QueryContext),
                null,
                numericExpression,
                null,
                null,
                parameter.QueryContext);
        }

        /// <inheritdoc />
        public object Visit(SqlRegexFunction regexFunctionExpression, object data)
        {
            var parameter = (ExpressionVisitParameter)data;

            if (regexFunctionExpression.Flags != null)
            {
                throw new NotImplementedException();
            }

            var text = CreateExpression(regexFunctionExpression.Text, parameter);
            var patternExpressionSet = CreateExpression(regexFunctionExpression.Pattern, parameter);

            if (!(patternExpressionSet.TypeExpression is ConstantExpression patternTypeExpression))
            {
                throw new NotImplementedException();
            }

            int patternTypeIndex = (int)patternTypeExpression.Value;
            var patternType = parameter.QueryContext.TypeCache.GetValueType(patternTypeIndex);

            bool producesAnError = patternType.Category != TypeCategories.SimpleLiteral;

            if (!(patternExpressionSet.StringExpression is ConstantExpression patternExpression))
            {
                throw new NotImplementedException();
            }

            var pattern = (string) patternExpression.Value;
            pattern = ProcessRegexPattern(pattern);

            var likeCondition = new LikeCondition(text.StringExpression, pattern);

            var notErrorCondition = new ConjunctionCondition(new[]
            {
                text.IsNotErrorCondition,
                patternExpressionSet.IsNotErrorCondition,
                // TODO: include flags
                producesAnError ? (IFilterCondition)new AlwaysFalseCondition() : new AlwaysTrueCondition(),
                new DisjunctionCondition(new[]
                {
                    new ComparisonCondition(text.TypeCategoryExpression,
                        new ConstantExpression((int) TypeCategories.StringLiteral, parameter.QueryContext),
                        ComparisonTypes.EqualTo),
                    new ComparisonCondition(text.TypeCategoryExpression,
                        new ConstantExpression((int) TypeCategories.SimpleLiteral, parameter.QueryContext),
                        ComparisonTypes.EqualTo)
                })
            });

            return new ConditionPart(notErrorCondition, likeCondition);
        }

        private string ProcessRegexPattern(string pattern)
        {
            // TODO: Improve this function
            var sb = new StringBuilder();

            for (int i = 0; i < pattern.Length; i++)
            {
                if (i == 0)
                {
                    if (pattern[i] == '^')
                    {
                        continue;
                    }
                    else
                    {
                        sb.Append("%");
                    }
                }

                var c = pattern[i];

                switch (c)
                {
                    case '%':
                        sb.Append("[%]");
                        break;
                    case '[':
                        sb.Append("[[]");
                        break;
                    case ']':
                        sb.Append("[]]");
                        break;
                    case '_':
                        sb.Append("[_]");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }

                if (i == pattern.Length - 1)
                {
                    if (pattern[i] == '$')
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }
                    else
                    {
                        sb.Append('%');
                    }
                }
            }

            return sb.ToString();
        }
    }
}