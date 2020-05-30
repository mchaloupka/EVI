using System;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Database.Base
{
    /// <summary>
    /// Base database representation
    /// </summary>
    public abstract class BaseSqlDb
        : ISqlDatabase
    {
        /// <summary>
        /// The query builder
        /// </summary>
        private readonly ISqlQueryBuilder _queryBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSqlDb" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <param name="queryTimeout">The time in seconds to wait for the command to execute.</param>
        protected BaseSqlDb(ISqlDbFactory factory, string connectionString, SqlType sqlType, int queryTimeout)
        {
            SqlType = sqlType;
            ConnectionString = connectionString;
            QueryTimeout = queryTimeout;

            _queryBuilder = factory.CreateSqlQueryBuilder();
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="relationalQuery">The relational model.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The SQL query</returns>
        public string GenerateQuery(RelationalQuery relationalQuery, IQueryContext context)
        {
            return _queryBuilder.GenerateQuery(relationalQuery, context);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query result reader.</returns>
        public abstract IQueryResultReader ExecuteQuery(string query);

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the type of the SQL connection.
        /// </summary>
        /// <value>The type of the SQL.</value>
        protected SqlType SqlType { get; }

        /// <summary>
        /// Gets the unquoted table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.String.</returns>
        public abstract string GetTableNameUnquoted(string tableName);

        /// <summary>
        /// Gets the name of table in the schema.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.String.</returns>
        public abstract string GetSchemaTableName(string tableName);

        /// <summary>
        /// Gets the unquoted column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>System.String.</returns>
        public abstract string GetColumnNameUnquoted(string columnName);

        /// <summary>
        /// Gets the SQL type for string.
        /// </summary>
        /// <value>The SQL type for string.</value>
        public abstract DataType SqlTypeForString { get; }

        /// <summary>
        /// Gets the SQL type for int.
        /// </summary>
        /// <value>The SQL type for int.</value>
        public abstract DataType SqlTypeForInt { get; }

        /// <inheritdoc />
        public abstract DataType SqlTypeForDouble { get; }

        /// <inheritdoc />
        public abstract DataType SqlTypeForBoolean { get; }

        /// <inheritdoc />
        public abstract DataType SqlTypeForDateTime { get; }

        /// <summary>
        /// Gets the natural RDF type for the SQL type <paramref name="dbType"/>
        /// </summary>
        public abstract Uri GetNaturalRdfType(string dbType);

        /// <inheritdoc />
        public abstract DataType GetCommonTypeForTwoColumns(DataType leftDataType, DataType rightDataType, out string neededCastLeft,
            out string neededCastRight);

        /// <inheritdoc />
        public abstract DbConnection CreateConnection();

        /// <summary>Gets or sets the wait time before terminating the attempt to execute a command and generating an error.</summary>
        /// <returns>The time in seconds to wait for the command to execute.</returns>
        public int QueryTimeout { get; }
    }
}
