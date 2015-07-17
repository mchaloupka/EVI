using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query.Source;

namespace Slp.r2rml4net.Storage.Database.Base
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
        private ISqlQueryBuilder _queryBuilder;

        /// <summary>
        /// The name generator
        /// </summary>
        private ISqlNameGenerator _nameGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSqlDb" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sqlType">Type of the SQL.</param>
        protected BaseSqlDb(ISqlDbFactory factory, string connectionString, SqlType sqlType)
        {
            SqlType = sqlType;
            ConnectionString = connectionString;

            _queryBuilder = factory.CreateSqlQueryBuilder();
            _nameGenerator = factory.CreateNameGenerator(this);
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="calculusModel">The SQL model.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The SQL query</returns>
        public string GenerateQuery(CalculusModel calculusModel, QueryContext context)
        {
            _nameGenerator.GenerateNames(calculusModel, context);
            return _queryBuilder.GenerateQuery(calculusModel, context);
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
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the type of the SQL connection.
        /// </summary>
        /// <value>The type of the SQL.</value>
        public SqlType SqlType { get; private set; }

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
    }
}
