using Microsoft.Extensions.Logging;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers
{
    /// <summary>
    /// Comparison of constants
    /// </summary>
    public class ConstantExpressionEqualityOptimizer
        : BaseRelationalOptimizer<object>
    {
        /// <summary>
        /// Creates an instance of <see cref="ConstantExpressionEqualityOptimizer"/>
        /// </summary>
        public ConstantExpressionEqualityOptimizer(ILogger<ConstantExpressionEqualityOptimizer> logger)
            : base(new ConstantExpressionEqualityOptimizerImplementation(), logger)
        {
        }

        /// <summary>
        /// The <see cref="ConstantExpressionEqualityOptimizerImplementation"/> optimizer implementation.
        /// </summary>
        public class ConstantExpressionEqualityOptimizerImplementation
            : BaseRelationalOptimizerImplementation<object>
        {
            /// <summary>
            /// Enumeration for the result of the <see cref="CheckComparison"/> method
            /// </summary>
            private enum EqualityResults
            {
                /// <summary>
                /// The operands are always equal
                /// </summary>
                Always,
                /// <summary>
                /// The operands may be equal
                /// </summary>
                Sometimes,
                /// <summary>
                /// The operands are never equal
                /// </summary>
                Never
            }

            /// <summary>
            /// Process the <see cref="ComparisonCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(ComparisonCondition toTransform, OptimizationContext data)
            {
                var leftOperand = toTransform.LeftOperand;
                var rightOperand = toTransform.RightOperand;

                var compareResult = CheckComparison(leftOperand, rightOperand, toTransform.ComparisonType);

                switch (compareResult)
                {
                    case EqualityResults.Always:
                        return new AlwaysTrueCondition();
                    case EqualityResults.Never:
                        return new AlwaysFalseCondition();
                    default:
                        return toTransform;
                }
            }

            /// <summary>
            /// Checks the equality of the operands.
            /// </summary>
            /// <param name="leftOperand">The left operand.</param>
            /// <param name="rightOperand">The right operand.</param>
            /// <param name="comparisonType">The comparison type</param>
            /// <returns>The <see cref="EqualityResults"/> of the check.</returns>
            private EqualityResults CheckComparison(IExpression leftOperand, IExpression rightOperand, ComparisonTypes comparisonType)
            {
                if (leftOperand is ConstantExpression leftConstantExpression && rightOperand is ConstantExpression rightConstantExpression && (comparisonType == ComparisonTypes.EqualTo || comparisonType == ComparisonTypes.NotEqualTo))
                {
                    if (AreConstantExpressionsEqual(leftConstantExpression, rightConstantExpression))
                    {
                        if (comparisonType == ComparisonTypes.EqualTo)
                        {
                            return EqualityResults.Always;
                        }
                        else
                        {
                            return EqualityResults.Never;
                        }
                    }
                    else
                    {
                        if (comparisonType == ComparisonTypes.EqualTo)
                        {
                            return EqualityResults.Never;
                        }
                        else
                        {
                            return EqualityResults.Always;
                        }
                    }
                }

                return EqualityResults.Sometimes;
            }

            /// <summary>
            /// Checks whether two constant expressions are equal or not.
            /// </summary>
            /// <param name="leftOperand">The left operand.</param>
            /// <param name="rightOperand">The right operand.</param>
            /// <returns>Returns <c>true</c> if the left operand is equal to the right one, <c>false</c> otherwise.</returns>
            private bool AreConstantExpressionsEqual(ConstantExpression leftOperand, ConstantExpression rightOperand)
            {
                return (leftOperand.SqlType.TypeName == rightOperand.SqlType.TypeName)
                    && (leftOperand.SqlString == rightOperand.SqlString);
            }
        }
    }
}
