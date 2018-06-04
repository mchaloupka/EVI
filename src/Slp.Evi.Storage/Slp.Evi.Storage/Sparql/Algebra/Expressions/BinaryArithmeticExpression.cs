using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Represents an arithmetic expression
    /// </summary>
    public class BinaryArithmeticExpression
        : ISparqlExpression
    {
        /// <summary>
        /// Gets the left operand.
        /// </summary>
        /// <value>The left operand.</value>
        public ISparqlExpression LeftOperand { get; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        /// <value>The right operand.</value>
        public ISparqlExpression RightOperand { get; }

        /// <summary>
        /// Gets the arithmetic operation.
        /// </summary>
        /// <value>The airthmetic operation.</value>
        public ArithmeticOperation Operation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryArithmeticExpression"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="operation">The arithmetic operation.</param>
        public BinaryArithmeticExpression(ISparqlExpression leftOperand, ISparqlExpression rightOperand, ArithmeticOperation operation)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operation = operation;
            NeededVariables = LeftOperand.NeededVariables.Union(RightOperand.NeededVariables).Distinct().ToArray();
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <inheritdoc />
        public IEnumerable<string> NeededVariables { get; }
    }
}