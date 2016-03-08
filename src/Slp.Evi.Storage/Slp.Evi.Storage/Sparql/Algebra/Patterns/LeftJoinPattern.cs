using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Left join
    /// </summary>
    public class LeftJoinPattern
        : IGraphPattern
    {
        // TODO: Add missing parts

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftJoinPattern"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        public LeftJoinPattern(IGraphPattern leftOperand, IGraphPattern rightOperand)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;

            Variables = LeftOperand.Variables
                .Union(RightOperand.Variables).Distinct().ToList();
        }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        /// <value>The right operand.</value>
        public IGraphPattern RightOperand { get; private set; }

        /// <summary>
        /// Gets the left operand.
        /// </summary>
        /// <value>The left operand.</value>
        public IGraphPattern LeftOperand { get; private set; }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IGraphPatternVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
