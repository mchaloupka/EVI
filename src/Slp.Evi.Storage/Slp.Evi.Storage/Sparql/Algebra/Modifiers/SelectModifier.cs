using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Sparql.Algebra.Modifiers
{
    /// <summary>
    /// Select modifier
    /// </summary>
    public class SelectModifier
        : IModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectModifier"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        /// <param name="sparqlVariables">The sparql variables.</param>
        public SelectModifier(ISparqlQuery innerQuery, IEnumerable<string> sparqlVariables)
        {
            InnerQuery = innerQuery;
            Variables = sparqlVariables;
        }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; }

        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        public ISparqlQuery InnerQuery { get; private set; }

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
