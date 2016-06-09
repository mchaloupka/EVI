using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;

namespace Slp.Evi.Storage.Sparql.PostProcess
{
    /// <summary>
    /// Interface for SPARQL optimization
    /// </summary>
    public interface ISparqlPostProcess
    {
        /// <summary>
        /// Processes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        ISparqlQuery Process(ISparqlQuery query, QueryContext context);
    }
}
