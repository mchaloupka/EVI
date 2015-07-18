using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Source;

namespace Slp.r2rml4net.Storage.Database
{
    /// <summary>
    /// Interface providing access to database specific
    /// </summary>
    public interface ISqlDatabase
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="relationalQuery">The relational model.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The SQL query</returns>
        string GenerateQuery(RelationalQuery relationalQuery, QueryContext context);

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query result reader.</returns>
        IQueryResultReader ExecuteQuery(string query);

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the type of the SQL connection.
        /// </summary>
        SqlType SqlType { get; }

        /// <summary>
        /// Gets the unquoted table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        string GetTableNameUnquoted(string tableName);

        /// <summary>
        /// Gets the name of table in the schema.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        string GetSchemaTableName(string tableName);

        /// <summary>
        /// Gets the unquoted column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        string GetColumnNameUnquoted(string columnName);

        /// <summary>
        /// Gets the SQL type for string.
        /// </summary>
        DataType SqlTypeForString { get; }

        /// <summary>
        /// Gets the SQL type for int.
        /// </summary>
        DataType SqlTypeForInt { get; }
    }
}
