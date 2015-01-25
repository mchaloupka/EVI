using System.Security.Cryptography.X509Certificates;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;

namespace Slp.r2rml4net.Storage.Sql
{
    /// <summary>
    /// Interface for database vendor
    /// </summary>
    public interface ISqlDb
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="sqlAlgebra">The SQL algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The SQL query</returns>
        string GenerateQuery(INotSqlOriginalDbSource sqlAlgebra, QueryContext context);

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The query result reader.</returns>
        IQueryResultReader ExecuteQuery(string query, QueryContext context);

        /// <summary>
        /// Determines whether the specified columns can be unioned.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="other">The other column.</param>
        /// <returns><c>true</c> if the specified columns can be unioned; otherwise, <c>false</c>.</returns>
        bool CanBeUnioned(ISqlColumn column, ISqlColumn other);

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
    }
}
