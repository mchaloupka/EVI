using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.DBSchema
{
    /// <summary>
    /// Interface for the provider for functions related to database schema.
    /// </summary>
    public interface IDbSchemaProvider
    {
        /// <summary>
        /// Gets the table information.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>DatabaseTable.</returns>
        /// <exception cref="System.Exception">Table not found in database schema</exception>
        DatabaseTable GetTableInfo(string tableName);
    }
}