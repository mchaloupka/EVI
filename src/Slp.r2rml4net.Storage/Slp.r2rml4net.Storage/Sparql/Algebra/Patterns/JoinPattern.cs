using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Patterns
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
            this.JoinedGraphPatterns = joinedGraphPatterns;
        }

        /// <summary>
        /// Gets the joined graph patterns.
        /// </summary>
        /// <value>The joined graph patterns.</value>
        public IEnumerable<IGraphPattern> JoinedGraphPatterns { get; private set; }
    }
}
