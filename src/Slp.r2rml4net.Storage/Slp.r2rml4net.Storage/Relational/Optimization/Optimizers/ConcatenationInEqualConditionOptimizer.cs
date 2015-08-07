using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers
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
            /// Process the <see cref="EqualExpressionCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(EqualExpressionCondition toTransform, OptimizationContext data)
            {
                var leftOperand = toTransform.LeftOperand;
                var rightOperand = toTransform.RightOperand;

                IFilterCondition result = null;

                if (leftOperand is ConcatenationExpression)
                {
                    result = ExpandEquals((ConcatenationExpression)leftOperand, rightOperand, data);
                }
                else if (rightOperand is ConcatenationExpression)
                {
                    result = ExpandEquals((ConcatenationExpression)rightOperand, leftOperand, data);
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

                int leftStart = 0;
                int rightStart = 0;
                StringBuilder leftPrefix = new StringBuilder();
                StringBuilder rightPrefix = new StringBuilder();

                int leftEnd = leftParts.Length - 1;
                int rightEnd = rightParts.Length - 1;
                StringBuilder leftSuffix = new StringBuilder();
                StringBuilder rightSuffix = new StringBuilder();

                List<IFilterCondition> conditions = new List<IFilterCondition>();

                while (true)
                {
                    bool performedAction = false;

                    GetPrefix(leftParts, leftPrefix, ref leftStart, leftEnd, leftSuffix);
                    GetPrefix(rightParts, rightPrefix, ref rightStart, rightEnd, rightSuffix);

                    GetSuffix(leftParts, leftStart, ref leftEnd, leftSuffix);
                    GetSuffix(rightParts, rightStart, ref rightEnd, rightSuffix);

                    var minSharedPrefix = Math.Min(leftPrefix.Length, rightPrefix.Length);

                    if (minSharedPrefix > 0)
                    {
                        var leftSubPrefix = leftPrefix.ToString().Substring(0, minSharedPrefix);
                        var rightSubPrefix = rightPrefix.ToString().Substring(0, minSharedPrefix);

                        leftPrefix.Remove(0, minSharedPrefix);
                        rightPrefix.Remove(0, minSharedPrefix);
                        performedAction = true;

                        if (leftSubPrefix != rightSubPrefix)
                        {
                            return new AlwaysFalseCondition();
                        }
                    }

                    var minSharedSuffix = Math.Min(rightSuffix.Length, leftSuffix.Length);

                    if (minSharedSuffix > 0)
                    {
                        var leftSubSuffix = leftSuffix.ToString().Substring(leftSuffix.Length - minSharedSuffix);
                        var rightSubSuffix = rightSuffix.ToString().Substring(rightSuffix.Length - minSharedSuffix);

                        leftSuffix.Remove(leftSuffix.Length - minSharedSuffix, minSharedSuffix);
                        rightSuffix.Remove(rightSuffix.Length - minSharedSuffix, minSharedSuffix);
                        performedAction = true;

                        if (leftSubSuffix != rightSubSuffix)
                        {
                            return new AlwaysFalseCondition();
                        }
                    }

                    if (!performedAction)
                    {
                        break;
                    }
                }

                var remainingLeftParts =
                    new List<IExpression>(GetRemainingParts(leftParts, leftPrefix, leftStart, leftEnd, leftSuffix, data.Context));

                var remainingRightParts =
                    new List<IExpression>(GetRemainingParts(rightParts, rightPrefix, rightStart, rightEnd, rightSuffix, data.Context));

                if (remainingLeftParts.Count > 0 || remainingRightParts.Count > 0)
                {
                    var leftExpression = GetExpression(data, remainingLeftParts);
                    var rightExpression = GetExpression(data, remainingRightParts);

                    conditions.Add(new EqualExpressionCondition(leftExpression, rightExpression));
                }

                if (conditions.Count == 0)
                {
                    return new AlwaysTrueCondition();
                }
                else if (conditions.Count == 1)
                {
                    return conditions[0];
                }
                else
                {
                    return new ConjunctionCondition(conditions);
                }
            }

            /// <summary>
            /// Gets the expression.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="remainingParts">The remaining parts.</param>
            private static IExpression GetExpression(OptimizationContext data, List<IExpression> remainingParts)
            {
                IExpression expression;
                if (remainingParts.Count == 0)
                {
                    expression = new ConstantExpression(string.Empty, data.Context);
                }
                else if (remainingParts.Count == 1)
                {
                    expression = remainingParts[0];
                }
                else
                {
                    expression = new ConcatenationExpression(remainingParts, data.Context.Db.SqlTypeForString);
                }

                return expression;
            }

            /// <summary>
            /// Gets the remaining parts.
            /// </summary>
            /// <param name="parts">The parts.</param>
            /// <param name="prefix">The prefix.</param>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="suffix">The suffix.</param>
            /// <param name="context">The context.</param>
            private IEnumerable<IExpression> GetRemainingParts(IExpression[] parts, StringBuilder prefix, int start, int end, StringBuilder suffix, QueryContext context)
            {
                if (prefix.Length > 0)
                {
                    yield return new ConstantExpression(prefix.ToString(), context);
                }

                for (int i = start; i <= end; i++)
                {
                    yield return parts[i];
                }

                if (suffix.Length > 0)
                {
                    yield return new ConstantExpression(suffix.ToString(), context);
                }
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

            /// <summary>
            /// Gets the prefix.
            /// </summary>
            /// <param name="parts">The parts.</param>
            /// <param name="prefix">The prefix.</param>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="suffix">The suffix.</param>
            private void GetPrefix(IExpression[] parts, StringBuilder prefix, ref int start, int end, StringBuilder suffix)
            {
                for (; start <= end; start++)
                {
                    var part = parts[start];

                    var constantExpression = part as ConstantExpression;
                    if (constantExpression != null)
                    {
                        prefix.Append(constantExpression.Value);

                        if (start == end)
                        {
                            suffix.Clear();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Gets the suffix.
            /// </summary>
            /// <param name="parts">The parts.</param>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="suffix">The suffix.</param>
            private void GetSuffix(IExpression[] parts, int start, ref int end, StringBuilder suffix)
            {
                for (; end > start; end--)
                {
                    var part = parts[end];

                    var constantExpression = part as ConstantExpression;
                    if (constantExpression != null)
                    {
                        suffix.Insert(0, constantExpression.Value);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
