using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Modifiers
{
    /// <summary>
    /// The SPARQL DISTINCT modifier implementation
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.IModifier" />
    public class DistinctModifier
        : IModifier
    {
        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        public ISparqlQuery InnerQuery { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctModifier"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        public DistinctModifier(ISparqlQuery innerQuery)
        {
            InnerQuery = innerQuery;
        }

        /// <inheritdoc />
        public IEnumerable<string> Variables => InnerQuery.Variables;

        /// <inheritdoc />
        public object Accept(IModifierVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
