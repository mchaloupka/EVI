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
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftJoinPattern"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="condition">The condition</param>
        public LeftJoinPattern(IGraphPattern leftOperand, IGraphPattern rightOperand, ISparqlCondition condition)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Condition = condition;

            Variables = LeftOperand.Variables
                .Union(RightOperand.Variables).Distinct().ToList();
        }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        /// <value>The right operand.</value>
        public IGraphPattern RightOperand { get; }

        /// <summary>
        /// Gets the condition.
        /// </summary>
        /// <value>The condition.</value>
        public ISparqlCondition Condition { get; }

        /// <summary>
        /// Gets the left operand.
        /// </summary>
        /// <value>The left operand.</value>
        public IGraphPattern LeftOperand { get; }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; }

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

        /// <summary>
        /// Gets the set of always bound variables.
        /// </summary>
        public IEnumerable<string> AlwaysBoundVariables => LeftOperand.AlwaysBoundVariables;
    }
}
