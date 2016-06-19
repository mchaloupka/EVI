using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Filter pattern
    /// </summary>
    public class FilterPattern
        : IGraphPattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterPattern"/> class.
        /// </summary>
        /// <param name="innerPattern">The inner pattern.</param>
        /// <param name="condition">The condition</param>
        public FilterPattern(IGraphPattern innerPattern, ISparqlCondition condition)
        {
            InnerPattern = innerPattern;
            Condition = condition;
        }

        /// <summary>
        /// Gets the inner pattern.
        /// </summary>
        /// <value>The inner pattern.</value>
        public IGraphPattern InnerPattern { get; }

        /// <summary>
        /// Gets the condition.
        /// </summary>
        public ISparqlCondition Condition { get; private set; }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables => InnerPattern.Variables;

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
        public IEnumerable<string> AlwaysBoundVariables => InnerPattern.AlwaysBoundVariables;
    }
}
