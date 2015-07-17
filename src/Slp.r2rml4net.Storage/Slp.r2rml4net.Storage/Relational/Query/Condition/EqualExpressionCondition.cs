using System;
using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Relational.Query.Condition
{
    /// <summary>
    /// The equal expression condition
    /// </summary>
    public class EqualExpressionCondition 
        : ICondition
    {
        /// <summary>
        /// Gets the left operand.
        /// </summary>
        /// <value>The left operand.</value>
        public IExpression LeftOperand { get; private set; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        /// <value>The right operand.</value>
        public IExpression RightOperand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualExpressionCondition"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        public EqualExpressionCondition(IExpression leftOperand, IExpression rightOperand)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}