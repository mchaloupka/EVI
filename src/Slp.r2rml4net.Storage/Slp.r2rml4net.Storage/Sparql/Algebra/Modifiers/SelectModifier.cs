using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Modifiers
{
    /// <summary>
    /// Select modifier
    /// </summary>
    public class SelectModifier
        : ISparqlQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectModifier"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        /// <param name="sparqlVariables">The sparql variables.</param>
        public SelectModifier(ISparqlQuery innerQuery, IEnumerable<string> sparqlVariables)
        {
            this.InnerQuery = innerQuery;
            this.Variables = sparqlVariables;
        }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; private set; }

        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        public ISparqlQuery InnerQuery { get; private set; }
    }
}
