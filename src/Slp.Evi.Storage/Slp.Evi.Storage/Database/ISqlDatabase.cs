using System;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Database
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
        string GenerateQuery(RelationalQuery relationalQuery, IQueryContext context);

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query result reader.</returns>
        IQueryResultReader ExecuteQuery(string query);

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

        /// <summary>
        /// Gets the SQL type for double.
        /// </summary>
        DataType SqlTypeForDouble { get; }

        /// <summary>
        /// Gets the SQL type for boolean.
        /// </summary>
        DataType SqlTypeForBoolean { get; }

        /// <summary>
        /// Gets the SQL type for date time.
        /// </summary>
        DataType SqlTypeForDateTime { get; }

        /// <summary>
        /// Gets the natural RDF type for the SQL type <paramref name="dbType"/>
        /// </summary>
        Uri GetNaturalRdfType(string dbType);

        /// <summary>
        /// Gets the nearest type these two types could be casted to for operations.
        /// </summary>
        DataType GetCommonTypeForTwoColumns(DataType leftDataType, DataType rightDataType, out string neededCastLeft, out string neededCastRight);

        /// <summary>
        /// Creates the connection to the database.
        /// </summary>
        DbConnection CreateConnection();
    }
}
