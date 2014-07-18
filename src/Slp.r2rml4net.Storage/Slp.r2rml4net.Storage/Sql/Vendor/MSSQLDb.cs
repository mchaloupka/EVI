using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    /// <summary>
    /// MS SQL database vendor
    /// </summary>
    public class MSSQLDb : BaseSqlDb
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSSQLDb"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public MSSQLDb(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The query result reader.</returns>
        public override IQueryResultReader ExecuteQuery(string query, Query.QueryContext context)
        {
            SqlConnection sqlConnection = new SqlConnection(this.connectionString);
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            command.CommandType = System.Data.CommandType.Text;
            command.Connection = sqlConnection;

            sqlConnection.Open();

            var reader = command.ExecuteReader();
            return new DataReaderWrapper(reader, () => sqlConnection.State == System.Data.ConnectionState.Open, () => sqlConnection.Close());
        }
    }
}
