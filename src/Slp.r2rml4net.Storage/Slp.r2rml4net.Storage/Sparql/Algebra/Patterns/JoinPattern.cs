using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;

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
            this.Variables = this.JoinedGraphPatterns.SelectMany(x => x.Variables)
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
    }
}
