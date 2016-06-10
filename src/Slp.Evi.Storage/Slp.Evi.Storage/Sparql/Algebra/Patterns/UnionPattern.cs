using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Union pattern
    /// </summary>
    public class UnionPattern
        : IGraphPattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoinPattern"/> class.
        /// </summary>
        /// <param name="unionedGraphPatterns">The unioned graph patterns.</param>
        public UnionPattern(IEnumerable<IGraphPattern> unionedGraphPatterns)
        {
            UnionedGraphPatterns = unionedGraphPatterns.ToArray();
            Variables = UnionedGraphPatterns.SelectMany(x => x.Variables)
                .Distinct().ToList();

            HashSet<string> stringSet = null;
            foreach (var unionedGraphPattern in UnionedGraphPatterns)
            {
                if (stringSet == null)
                {
                    stringSet = new HashSet<string>(unionedGraphPattern.AlwaysBoundVariables);
                }
                else
                {
                    stringSet.IntersectWith(unionedGraphPattern.AlwaysBoundVariables);
                }
            }
            AlwaysBoundVariables = (stringSet ?? new HashSet<string>()).ToArray();
        }

        /// <summary>
        /// Gets the unioned graph patterns.
        /// </summary>
        /// <value>The unioned graph patterns.</value>
        public IEnumerable<IGraphPattern> UnionedGraphPatterns { get; }

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
        public IEnumerable<string> AlwaysBoundVariables { get; }
    }
}
