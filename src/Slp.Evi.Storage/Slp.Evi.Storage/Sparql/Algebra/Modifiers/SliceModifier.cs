using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Modifiers
{
    /// <summary>
    /// The slice modifier (LIMIT and OFFSET)
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.IModifier" />
    public class SliceModifier
        : IModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByModifier"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        /// <param name="sparqlVariables">The sparql variables.</param>
        /// <param name="limit">The LIMIT value</param>
        /// <param name="offset">The OFFSET value</param>
        public SliceModifier(ISparqlQuery innerQuery, IEnumerable<string> sparqlVariables, int? limit, int? offset)
        {
            InnerQuery = innerQuery;
            Variables = sparqlVariables;
            Limit = limit;
            Offset = offset;
        }

        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        public ISparqlQuery InnerQuery { get; }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; }

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>The offset.</value>
        public int? Offset { get; }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        /// <value>The limit.</value>
        public int? Limit { get; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IModifierVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
