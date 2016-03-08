using System.Collections.Generic;

namespace Slp.Evi.Storage.Sparql.Algebra
{
    /// <summary>
    /// Base interface for all SPARQL queries
    /// </summary>
    public interface ISparqlQuery
    {
        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        IEnumerable<string> Variables { get; }
    }
}
