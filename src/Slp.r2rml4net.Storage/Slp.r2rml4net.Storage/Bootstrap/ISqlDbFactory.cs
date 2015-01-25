using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Vendor;

namespace Slp.r2rml4net.Storage.Bootstrap
{
    /// <summary>
    /// Factory for database related things
    /// </summary>
    public interface ISqlDbFactory
    {
        /// <summary>
        /// Creates the SQL database.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        ISqlDb CreateSqlDb(string connectionString);

        /// <summary>
        /// Creates the SQL query builder.
        /// </summary>
        BaseSqlQueryBuilder CreateSqlQueryBuilder();

        /// <summary>
        /// Creates the name generator.
        /// </summary>
        /// <param name="db"></param>
        BaseSqlNameGenerator CreateNameGenerator(ISqlDb db);
    }
}
