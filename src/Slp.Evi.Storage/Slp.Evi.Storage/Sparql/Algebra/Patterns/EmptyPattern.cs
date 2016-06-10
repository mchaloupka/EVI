using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Empty pattern
    /// </summary>
    public class EmptyPattern
        : IGraphPattern
    {
        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables => new string[] { };

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
        public IEnumerable<string> AlwaysBoundVariables => new string[] { };
    }
}
