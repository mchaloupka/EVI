using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Join pattern
    /// </summary>
    public class JoinPattern
        : IGraphPattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoinPattern"/> class.
        /// </summary>
        /// <param name="joinedGraphPatterns">The joined graph patterns.</param>
        public JoinPattern(IEnumerable<IGraphPattern> joinedGraphPatterns)
        {
            JoinedGraphPatterns = joinedGraphPatterns;
            Variables = JoinedGraphPatterns.SelectMany(x => x.Variables)
                .Distinct().ToList();
        }

        /// <summary>
        /// Gets the joined graph patterns.
        /// </summary>
        /// <value>The joined graph patterns.</value>
        public IEnumerable<IGraphPattern> JoinedGraphPatterns { get; private set; }

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
