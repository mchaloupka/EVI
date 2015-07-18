using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Source;

namespace Slp.r2rml4net.Storage.Database
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
