using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Relational.Database.Base;
using Slp.r2rml4net.Storage.Relational.Database.Reader;

namespace Slp.r2rml4net.Storage.Relational.Database.Vendor.MsSql
{
    /// <summary>
    /// MS SQL database vendor
    /// </summary>
    public class MsSqlDb
        : BaseSqlDb
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlDb"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="connectionString">The connection string.</param>
        public MsSqlDb(ISqlDbFactory factory, string connectionString) 
            : base(factory, connectionString, SqlType.SqlServer)
        {

        }

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
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query result reader.</returns>
        public override IQueryResultReader ExecuteQuery(string query)
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
        /// Gets the SQL type for string.
        /// </summary>
        /// <value>The SQL type for string.</value>
        public override DataType SqlTypeForString
        {
            get { return new DataType("nvarchar(max)", "System.String"); }
        }

        /// <summary>
        /// Gets the SQL type for int.
        /// </summary>
        /// <value>The SQL type for int.</value>
        public override DataType SqlTypeForInt
        {
            get { return new DataType("int", "System.Int32"); }
        }
    }
}
