using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Common.Optimization.PatternMatching;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers
{
    /// <summary>
    /// CONCAT in equal optimization
    /// </summary>
    public class ConcatenationInEqualConditionOptimizer
        : BaseRelationalOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatenationInEqualConditionOptimizer"/> class.
        /// </summary>
        public ConcatenationInEqualConditionOptimizer() 
            : base(new ConcatenationInEqualConditionOptimizerImplementation())
        { }

        /// <summary>
        /// The <see cref="ConcatenationInEqualConditionOptimizer"/> optimizer implementation.
        /// </summary>
        private class ConcatenationInEqualConditionOptimizerImplementation
            : BaseRelationalOptimizerImplementation<object>
        {
            /// <summary>
            /// The <see cref="PatternComparer"/> used in this class.
            /// </summary>
            private readonly PatternComparer _comparer;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConcatenationInEqualConditionOptimizerImplementation"/> class.
            /// </summary>
            public ConcatenationInEqualConditionOptimizerImplementation()
            {
                _comparer = new PatternComparer();
            }

            /// <summary>
            /// Process the <see cref="ComparisonCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(ComparisonCondition toTransform, OptimizationContext data)
            {
                IFilterCondition result = null;

                if (toTransform.ComparisonType == ComparisonTypes.EqualTo ||
                    toTransform.ComparisonType == ComparisonTypes.NotEqualTo)
                {
                    var leftOperand = toTransform.LeftOperand;
                    var rightOperand = toTransform.RightOperand;

                    if (leftOperand is ConcatenationExpression)
                    {
                        result = ExpandEquals((ConcatenationExpression)leftOperand, rightOperand, data);
                    }
                    else if (rightOperand is ConcatenationExpression)
                    {
                        result = ExpandEquals((ConcatenationExpression)rightOperand, leftOperand, data);
                    }

                    if (result != null && toTransform.ComparisonType == ComparisonTypes.NotEqualTo)
                    {
                        result = new NegationCondition(result);
                    }
                }

                return result ?? toTransform;
            }

            /// <summary>
            /// Expands the equals operator.
            /// </summary>
            /// <param name="leftOperand">The left operand.</param>
            /// <param name="rightOperand">The right operand.</param>
            /// <param name="data">The context.</param>
            private IFilterCondition ExpandEquals(ConcatenationExpression leftOperand, IExpression rightOperand, OptimizationContext data)
            {
                if (rightOperand is ConcatenationExpression)
                {
                    return ExpandEquals(leftOperand, (ConcatenationExpression)rightOperand, data);
                }
                else if (rightOperand is ConstantExpression)
                {
                    return ExpandEquals(leftOperand, (ConstantExpression)rightOperand, data);
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Expands the equals operator.
            /// </summary>
            /// <param name="leftOperand">The left operand.</param>
            /// <param name="rightOperand">The right operand.</param>
            /// <param name="data">The context.</param>
            private IFilterCondition ExpandEquals(ConcatenationExpression leftOperand, ConstantExpression rightOperand, OptimizationContext data)
            {
                return ExpandEquals(leftOperand, new ConcatenationExpression(new List<IExpression>() { rightOperand }, data.Context.Db.SqlTypeForString), data);
            }

            /// <summary>
            /// Expands the equals operator.
            /// </summary>
            /// <param name="leftOperand">The left operand.</param>
            /// <param name="rightOperand">The right operand.</param>
            /// <param name="data">The context.</param>
            private IFilterCondition ExpandEquals(ConcatenationExpression leftOperand, ConcatenationExpression rightOperand, OptimizationContext data)
            {
                var leftParts = leftOperand.InnerExpressions.ToArray();
                var rightParts = rightOperand.InnerExpressions.ToArray();

                return ProcessConcatenation(leftParts, rightParts, data);
            }

            /// <summary>
            /// Processes the concatenation.
            /// </summary>
            /// <param name="leftParts">The left parts.</param>
            /// <param name="rightParts">The right parts.</param>
            /// <param name="data">The context.</param>
            private IFilterCondition ProcessConcatenation(IExpression[] leftParts, IExpression[] rightParts, OptimizationContext data)
            {
                if (!CanOptimize(leftParts) || !CanOptimize(rightParts))
                    return null;

                var leftPattern = GetPattern(leftParts);
                var rightPattern = GetPattern(rightParts);

                var compareResult = _comparer.Compare(leftPattern, rightPattern);

                if (compareResult.AlwaysMatch)
                {
                    return new AlwaysTrueCondition();
                }
                else if (compareResult.NeverMatch)
                {
                    return new AlwaysFalseCondition();
                }
                else if (compareResult.Conditions.Length == 0)
                {
                    throw new InvalidOperationException("CompareResult returned zero conditions");
                }
                else if (compareResult.Conditions.Length == 1)
                {
                    return ConvertToCondition(compareResult.Conditions[0], data.Context);
                }
                else
                {
                    return new ConjunctionCondition(compareResult.Conditions.Select(matchCondition => ConvertToCondition(matchCondition, data.Context)).ToArray());
                }
            }

            /// <summary>
            /// Converts to condition.
            /// </summary>
            /// <param name="matchCondition">The match condition to convert.</param>
            /// <param name="context">The context.</param>
            private IFilterCondition ConvertToCondition(MatchCondition matchCondition, IQueryContext context)
            {
                if (matchCondition.IsAlwaysFalse)
                {
                    return new AlwaysFalseCondition();
                }
                else
                {
                    return new ComparisonCondition(ConvertToExpression(matchCondition.LeftPattern, context),
                        ConvertToExpression(matchCondition.RightPattern, context), ComparisonTypes.EqualTo);
                }
            }

            /// <summary>
            /// Converts to expression.
            /// </summary>
            /// <param name="pattern">The pattern to convert.</param>
            /// <param name="context">The context.</param>
            private IExpression ConvertToExpression(Pattern pattern, IQueryContext context)
            {
                bool isIri = pattern.IsIriEscaped;

                if (pattern.PatternItems.Length == 0)
                {
                    return new ConstantExpression(string.Empty, context);
                }
                else if (pattern.PatternItems.Length == 1)
                {
                    return ConvertToExpression(pattern.PatternItems[0], isIri, context);
                }
                else
                {
                    return new ConcatenationExpression(pattern.PatternItems.Select(x => ConvertToExpression(x, isIri, context)).ToList(), context.Db.SqlTypeForString);
                }
            }

            /// <summary>
            /// Converts to expression.
            /// </summary>
            /// <param name="patternItem">The pattern item.</param>
            /// <param name="isIriEscaped">if set to <c>true</c> the value is iri escaped.</param>
            /// <param name="context">The context.</param>
            /// <returns>IExpression.</returns>
            private IExpression ConvertToExpression(PatternItem patternItem, bool isIriEscaped, IQueryContext context)
            {
                if (patternItem.IsConstant)
                {
                    return new ConstantExpression(patternItem.Text, context);
                }
                else
                {
                    return new ColumnExpression(patternItem.RelationalColumn, isIriEscaped);
                }
            }

            /// <summary>
            /// Gets the pattern.
            /// </summary>
            /// <param name="parts">The expression to build the pattern.</param>
            private static Pattern GetPattern(IExpression[] parts)
            {
                var isIriEscaped = parts.OfType<ColumnExpression>().All(x => x.IsUri);

                var patternItems = parts.Select(x =>
                {
                    if (x is ConstantExpression)
                    {
                        return new PatternItem(((ConstantExpression)x).Value.ToString());
                    }
                    else if (x is ColumnExpression)
                    {
                        return new PatternItem()
                        {
                            RelationalColumn = ((ColumnExpression)x).CalculusVariable
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot process anything else than constant and column expressions.");
                    }
                });

                return new Pattern(isIriEscaped, patternItems);
            }

            /// <summary>
            /// Determines whether we can optimize the specified parts.
            /// </summary>
            /// <param name="parts">The parts.</param>
            /// <returns><c>true</c> if we can optimize the specified parts; otherwise, <c>false</c>.</returns>
            private bool CanOptimize(IExpression[] parts)
            {
                var isAnyNotColumnAndNotConstant = parts
                    .Where(x => !(x is ConstantExpression))
                    .Any(x => !(x is ColumnExpression));

                var isAnyNotStringConstants = parts
                    .OfType<ConstantExpression>()
                    .Any(x => !(x.Value is string));

                return !isAnyNotStringConstants && !isAnyNotColumnAndNotConstant;
            }
        }
    }
}
