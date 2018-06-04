using System;
using System.Collections.Generic;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Database;

namespace Slp.Evi.Storage.DBSchema
{
    /// <summary>
    /// Provider for functions related to database schema.
    /// </summary>
    public class DbSchemaProvider : IDbSchemaProvider
    {
        /// <summary>
        /// The provided database
        /// </summary>
        private readonly ISqlDatabase _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSchemaProvider"/> class.
        /// </summary>
        /// <param name="db">The provided database.</param>
        public DbSchemaProvider(ISqlDatabase db)
        {
            _db = db;
            _tableCache = new Dictionary<string, DatabaseTable>();

            ReadDatabaseSchema();
        }

        /// <summary>
        /// Reads the database schema.
        /// </summary>
        private void ReadDatabaseSchema()
        {
            using (var connection = _db.CreateConnection())
            using (var dbReader = new DatabaseReader(connection))
            {
                var schema = dbReader.ReadAll();

                foreach (var table in schema.Tables)
                {
                    _tableCache.Add(table.Name, table);
                }

                connection.Close();
            }
        }

        /// <summary>
        /// The table info cache
        /// </summary>
        private readonly Dictionary<string, DatabaseTable> _tableCache;

        /// <summary>
        /// Gets the table information.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>DatabaseTable.</returns>
        /// <exception cref="System.Exception">Table not found in database schema</exception>
        public DatabaseTable GetTableInfo(string tableName)
        {
            var schemaTableName = _db.GetSchemaTableName(tableName);

            if (_tableCache.ContainsKey(schemaTableName))
                return _tableCache[schemaTableName];

            throw new Exception("Table not found in database schema");
        }
    }
}
