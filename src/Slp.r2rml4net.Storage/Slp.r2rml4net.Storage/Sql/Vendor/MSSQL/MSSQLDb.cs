using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Query;

namespace Slp.r2rml4net.Storage.Sql.Vendor.MSSQL
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
        /// The start delimiters
        /// </summary>
        private static readonly char[] StartDelimiters = new[] { '`', '\"', '[' };

        /// <summary>
        /// The end delimiters
        /// </summary>
        private static readonly char[] EndDelimiters = new[] { '`', '\"', ']' };

        /// <summary>
        /// Gets the unquoted table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public override string GetTableNameUnquoted(string tableName)
        {
            return GetColumnNameUnquoted(tableName);
        }

        /// <summary>
        /// Gets the unquoted column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public override string GetColumnNameUnquoted(string columnName)
        {
            return columnName.TrimStart(StartDelimiters).TrimEnd(EndDelimiters);
        }

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
            return new DataReaderWrapper(this, reader, () => sqlConnection.State == ConnectionState.Open, () => sqlConnection.Close());
        }
    }
}
