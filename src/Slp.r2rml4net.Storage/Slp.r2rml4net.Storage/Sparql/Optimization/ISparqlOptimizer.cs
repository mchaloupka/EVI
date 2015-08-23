using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;

namespace Slp.r2rml4net.Storage.Sparql.Optimization
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
