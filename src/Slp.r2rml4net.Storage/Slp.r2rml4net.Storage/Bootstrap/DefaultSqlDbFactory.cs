using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Vendor;
using Slp.r2rml4net.Storage.Sql.Vendor.MSSQL;

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
        public virtual ISqlDb CreateSqlDb(string connectionString)
        {
            return new MssqlDb(connectionString, this);
        }


        /// <summary>
        /// Creates the SQL query builder.
        /// </summary>
        public virtual BaseSqlQueryBuilder CreateSqlQueryBuilder()
        {
            return new BaseSqlQueryBuilder();
        }

        /// <summary>
        /// Creates the name generator.
        /// </summary>
        /// <param name="db">The database</param>
        public virtual BaseSqlNameGenerator CreateNameGenerator(ISqlDb db)
        {
            return new BaseSqlNameGenerator(db);
        }
    }
}
