using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Relational.Optimization
{
    /// <summary>
    /// Interface for relational optimization
    /// </summary>
    public interface IRelationalOptimizer
    {
        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        RelationalQuery Optimize(RelationalQuery query, QueryContext context);
    }
}
