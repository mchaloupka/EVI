using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;

namespace Slp.Evi.Storage.Sparql.Optimization
{
    /// <summary>
    /// Interface for SPARQL optimization
    /// </summary>
    public interface ISparqlOptimizer
    {
        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        ISparqlQuery Optimize(ISparqlQuery query, QueryContext context);
    }
}
