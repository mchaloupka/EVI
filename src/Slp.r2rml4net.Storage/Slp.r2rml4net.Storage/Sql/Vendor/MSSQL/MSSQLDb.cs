using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
        private static readonly char[] StartQuoting = { '`', '\"', '[' };

        /// <summary>
        /// The end delimiters
        /// </summary>
        private static readonly char[] EndQuoting = { '`', '\"', ']' };

        /// <summary>
        /// The middle delimiters
        /// </summary>
        private static readonly char[] MiddleDelimiters = { '.' };

        /// <summary>
        /// Gets the SQL type for concatenation.
        /// </summary>
        public override DataType SqlTypeForConcatenation
        {
            get { return new DataType("nvarchar(max)", "System.String"); }
        }

        /// <summary>
        /// Gets the SQL type for decider.
        /// </summary>
        public override DataType SqlTypeForDecider
        {
            get { return new DataType("int", "System.Int32"); }
        }

        /// <summary>
        /// Gets the unquoted table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public override string GetTableNameUnquoted(string tableName)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder curPart = new StringBuilder();

            foreach (var c in tableName)
            {
                if (MiddleDelimiters.Contains(c))
                {
                    sb.Append(GetColumnNameUnquoted(curPart.ToString()));
                    sb.Append(c);
                    curPart.Clear();
                }
                else
                {
                    curPart.Append(c);
                }
            }

            sb.Append(GetColumnNameUnquoted(curPart.ToString()));

            return sb.ToString();
        }

        /// <summary>
        /// Gets the name of table in the schema.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public override string GetSchemaTableName(string tableName)
        {
            var lastPart = tableName.Split(MiddleDelimiters).Last();
            return GetTableNameUnquoted(lastPart);
        }

        /// <summary>
        /// Gets the unquoted column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public override string GetColumnNameUnquoted(string columnName)
        {
            return columnName.TrimStart(StartQuoting).TrimEnd(EndQuoting);
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
