using Slp.Evi.Storage.Database;

namespace Slp.Evi.Storage.Bootstrap
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
    }
}
