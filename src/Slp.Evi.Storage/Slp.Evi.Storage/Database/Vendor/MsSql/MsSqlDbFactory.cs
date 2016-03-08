using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database.Base;

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
        public virtual ISqlDatabase CreateSqlDb(string connectionString)
        {
            return new MsSqlDb(this, connectionString);
        }

        /// <summary>
        /// Creates the SQL query builder.
        /// </summary>
        public virtual ISqlQueryBuilder CreateSqlQueryBuilder()
        {
            return new BaseSqlQueryBuilder();
        }
    }
}
