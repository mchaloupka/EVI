using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

            try
            {
                var reader = command.ExecuteReader();
                return new DataReaderWrapper(this, reader, () => sqlConnection.State == ConnectionState.Open, () =>
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                });
            }
            catch
            {
                try
                {
                    sqlConnection.Close();
                }
                catch
                {
                    // Exception ignored (the previously thrown exception is more important)
                }

                throw;
            }
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

        /// <inheritdoc />
        public override DataType GetCommonTypeForTwoColumns(DataType leftDataType, DataType rightDataType, out string neededCastLeft, out string neededCastRight)
        {
            neededCastLeft = null;
            neededCastRight = null;

            if (leftDataType.TypeName == rightDataType.TypeName)
            {
                return leftDataType;
            }

            var leftTypeName = leftDataType.TypeName.ToLowerInvariant();
            var rightTypeName = rightDataType.TypeName.ToLowerInvariant();

            if (leftDataType.IsNumeric && rightDataType.IsNumeric)
            {
                if ((leftTypeName == "float") || (leftTypeName == "real")
                    || (rightTypeName == "float") || (rightTypeName == "real"))
                {
                    if (leftTypeName != "float" && leftTypeName != "real")
                        neededCastLeft = "float";
                    if (rightTypeName != "float" && rightTypeName != "real")
                        neededCastRight = "float";

                    return new DataType("float", "System.Double");
                }

                var intTypes = new List<string>()
                {
                    "bit",
                    "tinyint",
                    "smallint",
                    "int",
                    "bigint"
                };

                var leftIndex = intTypes.FindIndex(x => x == leftTypeName);
                var rightIndex = intTypes.FindIndex(x => x == rightTypeName);

                if (leftIndex >= 0 && rightIndex >= 0)
                {
                    var neededType = intTypes[Math.Min(leftIndex, rightIndex)];

                    if (leftIndex < 0)
                        neededCastLeft = neededType;

                    if (rightIndex < 0)
                        neededCastRight = neededType;

                    return new DataType(intTypes[Math.Min(leftIndex, rightIndex)], "System.Int64");
                }
                else
                {
                    if (!leftDataType.IsNumeric)
                        neededCastLeft = "numeric(38,8)";
                    if (!rightDataType.IsNumeric)
                        neededCastRight = "numeric(38,8)";

                    return new DataType("numeric(38,8)", "System.Decimal");
                }
            }
            else if (leftDataType.IsDateTime && rightDataType.IsDateTime)
            {
                if (!leftDataType.IsDateTime)
                    neededCastLeft = "datetime2";

                if (!rightDataType.IsDateTime)
                    neededCastRight = "datetime2";

                return new DataType("datetime2", "System.DateTime");
            }
            else
            {
                if (!leftDataType.IsString)
                    neededCastLeft = "nvarchar(MAX)";
                if (!rightDataType.IsString)
                    neededCastRight = "nvarchar(MAX)";

                return new DataType("nvarchar(MAX)", "System.String");
            }
        }

        /// <inheritdoc />
        public override DbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
