using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Relational.PostProcess
{
    /// <summary>
    /// Interface for relational optimization
    /// </summary>
    public interface IRelationalPostProcess
    {
        /// <summary>
        /// Processes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        RelationalQuery Process(RelationalQuery query, QueryContext context);
    }
}
