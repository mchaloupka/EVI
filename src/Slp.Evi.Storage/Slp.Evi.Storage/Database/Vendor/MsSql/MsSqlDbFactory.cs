using Slp.Evi.Storage.Bootstrap;

namespace Slp.Evi.Storage.Database.Vendor.MsSql
{
    /// <summary>
    /// Sql factory for MS SQL
    /// </summary>
    public class MsSqlDbFactory
        : ISqlDbFactory
    {
        /// <summary>
        /// Creates the SQL database connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="queryTimeout">The time in seconds to wait for the command to execute.</param>
        public virtual ISqlDatabase CreateSqlDb(string connectionString, int queryTimeout)
        {
            return new MsSqlDb(this, connectionString, queryTimeout);
        }

        /// <summary>
        /// Creates the SQL query builder.
        /// </summary>
        public virtual ISqlQueryBuilder CreateSqlQueryBuilder()
        {
            return new MsSqlQueryBuilder();
        }
    }
}
