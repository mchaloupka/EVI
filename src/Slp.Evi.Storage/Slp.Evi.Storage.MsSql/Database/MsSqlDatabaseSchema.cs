using System;
using System.Collections.Generic;
using System.Data.Common;
using DatabaseSchemaReader;
using Slp.Evi.Common.DatabaseConnection;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlDatabaseSchema
        : ISqlDatabaseSchema
    {
        private readonly Dictionary<string, MsSqlTable> _tables;

        private MsSqlDatabaseSchema(Dictionary<string, MsSqlTable> tables)
        {
            _tables = tables;
        }

        public static MsSqlDatabaseSchema CreateFromDatabase(DbConnection connection)
        {
            var tables = new Dictionary<string, MsSqlTable>();

            using (var dbReader = new DatabaseReader(connection))
            {
                var schema = dbReader.ReadAll();

                foreach (var tableSchema in schema.Tables)
                {
                    tables.Add(tableSchema.Name, MsSqlTable.CreateFromDatabase(tableSchema));
                }
            }

            return new MsSqlDatabaseSchema(tables);
        }

        /// <inheritdoc />
        public string NormalizeTableName(string tableName)
        {
            if (_tables.ContainsKey(tableName))
            {
                return tableName;
            }
            else
            {
                throw new ArgumentException($"Table ${tableName} was not found in the database");
            }
        }

        /// <inheritdoc />
        public LiteralValueType DetectDefaultRdfType(string tableName, string columnName)
        {
            if (_tables.TryGetValue(tableName, out var table))
            {
                return table.DetectDefaultRdfType(columnName);
            }
            else
            {
                throw new ArgumentException($"Table ${tableName} was not found in the database");
            }
        }
    }
}
