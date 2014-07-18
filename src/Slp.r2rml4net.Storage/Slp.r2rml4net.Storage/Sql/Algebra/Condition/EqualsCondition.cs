using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    /// <summary>
    /// Equals condition
    /// </summary>
    public class EqualsCondition : ICondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsCondition"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        public EqualsCondition(IExpression leftOperand, IExpression rightOperand)
        {
            this.LeftOperand = leftOperand;
            this.RightOperand = rightOperand;
        }

        /// <summary>
        /// Gets or sets the right operand.
        /// </summary>
        /// <value>The right operand.</value>
        public IExpression RightOperand { get; set; }

        /// <summary>
        /// Gets or sets the left operand.
        /// </summary>
        /// <value>The left operand.</value>
        public IExpression LeftOperand { get; set; }

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

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new EqualsCondition((IExpression)this.LeftOperand.Clone(), (IExpression)this.RightOperand.Clone());
        }
    }
}
