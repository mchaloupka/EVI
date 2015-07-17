using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Database.Vendor.MsSql;

namespace Slp.r2rml4net.Storage.Bootstrap
{
    /// <summary>
    /// Class DefaultSqlDbFactory.
    /// </summary>
    public class DefaultSqlDbFactory : ISqlDbFactory
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
            return null;
        }

        /// <summary>
        /// Creates the name generator.
        /// </summary>
        /// <param name="db">The database</param>
        public virtual ISqlNameGenerator CreateNameGenerator(ISqlDatabase db)
        {
            return null;
        }
    }
}
