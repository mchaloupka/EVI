using System.Diagnostics;
using Slp.Evi.Storage.Common.Algebra;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Representing comparison condition
    /// </summary>
    public class ComparisonExpression 
        : ISparqlCondition
    {
        /// <summary>
        /// Gets the left operand.
        /// </summary>
        /// <value>The left operand.</value>
        public ISparqlExpression LeftOperand { get; private set; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        /// <value>The right operand.</value>
        public ISparqlExpression RightOperand { get; private set; }

        /// <summary>
        /// Gets the type of the comparison.
        /// </summary>
        /// <value>The type of the comparison.</value>
        public ComparisonTypes ComparisonType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonExpression"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        public ComparisonExpression(ISparqlExpression leftOperand, ISparqlExpression rightOperand, ComparisonTypes comparisonType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            ComparisonType = comparisonType;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
