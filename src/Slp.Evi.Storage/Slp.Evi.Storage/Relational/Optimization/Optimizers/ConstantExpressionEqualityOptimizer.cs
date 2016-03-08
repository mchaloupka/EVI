using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers
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
        public ConstantExpressionEqualityOptimizer() 
            : base(new ConstantExpressionEqualityOptimizerImplementation())
        {
        }

        /// <summary>
        /// The <see cref="ConstantExpressionEqualityOptimizerImplementation"/> optimizer implementation.
        /// </summary>
        public class ConstantExpressionEqualityOptimizerImplementation
            : BaseRelationalOptimizerImplementation<object>
        {
            /// <summary>
            /// Enumeration for the result of the <see cref="CheckEquality"/> method
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
            /// Process the <see cref="EqualExpressionCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(EqualExpressionCondition toTransform, OptimizationContext data)
            {
                var leftOperand = toTransform.LeftOperand;
                var rightOperand = toTransform.RightOperand;

                var compareResult = CheckEquality(leftOperand, rightOperand, data);

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
            /// <param name="data">The passed data.</param>
            /// <returns>The <see cref="EqualityResults"/> of the check.</returns>
            private EqualityResults CheckEquality(IExpression leftOperand, IExpression rightOperand, OptimizationContext data)
            {
                if (leftOperand is ConstantExpression && rightOperand is ConstantExpression)
                {
                    if (AreConstantExpressionsEqual((ConstantExpression) leftOperand, (ConstantExpression) rightOperand))
                    {
                        return EqualityResults.Always;
                    }
                    else
                    {
                        return EqualityResults.Never;
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
