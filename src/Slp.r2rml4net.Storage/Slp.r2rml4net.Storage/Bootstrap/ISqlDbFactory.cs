using Slp.r2rml4net.Storage.Relational.Database;

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
        ISqlDatabase CreateSqlDb(string connectionString);

        /// <summary>
        /// Creates the SQL query builder.
        /// </summary>
        ISqlQueryBuilder CreateSqlQueryBuilder();

        /// <summary>
        /// Creates the name generator.
        /// </summary>
        /// <param name="db"></param>
        ISqlNameGenerator CreateNameGenerator(ISqlDatabase db);
    }
}
