using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database.Base;
using Slp.Evi.Storage.Database.Reader;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Database.Vendor.MsSql
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
        public override DataType SqlTypeForString => new DataType("nvarchar(max)", "System.String");

        /// <summary>
        /// Gets the SQL type for int.
        /// </summary>
        /// <value>The SQL type for int.</value>
        public override DataType SqlTypeForInt => new DataType("int", "System.Int32");

        /// <inheritdoc />
        public override DataType SqlTypeForDouble => new DataType("float", "System.Double");

        /// <inheritdoc />
        public override DataType SqlTypeForBoolean => new DataType("bit", "System.Boolean");

        /// <inheritdoc />
        public override DataType SqlTypeForDateTime => new DataType("datetime2", "System.DateTime");

        /// <summary>
        /// Gets the natural RDF type for the SQL type <paramref name="dbType"/>
        /// </summary>
        public override Uri GetNaturalRdfType(string dbType)
        {
            switch (dbType.ToLower(CultureInfo.InvariantCulture))
            {
                case "nvarchar":
                case "varchar":
                case "nchar":
                case "char":
                case "text":
                case "ntext":
                    return null;
                case "bigint":
                case "int":
                case "smallint":
                case "tinyint":
                    return EviConstants.XsdInteger;
                case "smallmoney":
                case "decimal":
                case "money":
                case "numeric":
                    return EviConstants.XsdDecimal;
                case "bit":
                    return EviConstants.XsdBoolean;
                case "float":
                case "real":
                    return EviConstants.XsdDouble;
                case "date":
                    return EviConstants.XsdDate;
                case "time":
                    return EviConstants.XsdTime;
                case "datetime":
                    return EviConstants.XsdDateTime;
                case "binary":
                case "varbinary":
                case "image":
                    return EviConstants.XsdHexBinary;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
