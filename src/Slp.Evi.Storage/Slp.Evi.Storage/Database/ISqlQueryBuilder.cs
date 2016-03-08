using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Database
{
    /// <summary>
    /// The SQL query builder
    /// </summary>
    public interface ISqlQueryBuilder
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="relationalQuery">The calculus model.</param>
        /// <param name="context">The context.</param>
        /// <returns>The query string.</returns>
        string GenerateQuery(RelationalQuery relationalQuery, QueryContext context);
    }
}
