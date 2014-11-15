using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Sql.ISqlDb CreateSQLDb(string connectionString);

        /// <summary>
        /// Creates the SQL query builder.
        /// </summary>
        Sql.Vendor.BaseSqlQueryBuilder CreateSQLQueryBuilder();

        /// <summary>
        /// Creates the name generator.
        /// </summary>
        Sql.Vendor.BaseSqlNameGenerator CreateNameGenerator();
    }
}
