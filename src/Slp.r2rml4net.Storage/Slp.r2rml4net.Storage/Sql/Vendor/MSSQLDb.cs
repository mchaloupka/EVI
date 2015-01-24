using System.Data;
using System.Data.SqlClient;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Query;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    /// <summary>
    /// MS SQL database vendor
    /// </summary>
    public class MssqlDb : BaseSqlDb
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlDb"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="defaultSqlDbFactory">The SQL database factory.</param>
        public MssqlDb(string connectionString, ISqlDbFactory defaultSqlDbFactory)
            : base(defaultSqlDbFactory, connectionString, SqlType.SqlServer)
        { }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The query result reader.</returns>
        public override IQueryResultReader ExecuteQuery(string query, QueryContext context)
        {
            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand
            {
                CommandText = query,
                CommandType = CommandType.Text,
                Connection = sqlConnection
            };

            sqlConnection.Open();

            var reader = command.ExecuteReader();
            return new DataReaderWrapper(reader, () => sqlConnection.State == ConnectionState.Open, () => sqlConnection.Close());
        }
    }
}
