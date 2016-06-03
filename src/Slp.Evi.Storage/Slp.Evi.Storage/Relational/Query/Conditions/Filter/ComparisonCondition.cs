using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// Class representing a comparison condition
    /// </summary>
    public class ComparisonCondition
        : IFilterCondition
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
        /// Gets the type of the comparison.
        /// </summary>
        /// <value>The type of the comparison.</value>
        public ComparisonTypes ComparisonType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonCondition"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        public ComparisonCondition(IExpression leftOperand, IExpression rightOperand, ComparisonTypes comparisonType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            ComparisonType = comparisonType;
            UsedCalculusVariables =
                leftOperand.UsedCalculusVariables.Union(rightOperand.UsedCalculusVariables).Distinct().ToArray();
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IFilterConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }
    }
}
