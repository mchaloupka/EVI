using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql.Algebra
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
