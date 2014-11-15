using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public virtual Sql.ISqlDb CreateSQLDb(string connectionString)
        {
            return new Slp.r2rml4net.Storage.Sql.Vendor.MSSQLDb(connectionString, this);
        }


        /// <summary>
        /// Creates the SQL query builder.
        /// </summary>
        public Sql.Vendor.BaseSqlQueryBuilder CreateSQLQueryBuilder()
        {
            return new Sql.Vendor.BaseSqlQueryBuilder(this);
        }

        /// <summary>
        /// Creates the name generator.
        /// </summary>
        public Sql.Vendor.BaseSqlNameGenerator CreateNameGenerator()
        {
            return new Sql.Vendor.BaseSqlNameGenerator(this);
        }
    }
}
