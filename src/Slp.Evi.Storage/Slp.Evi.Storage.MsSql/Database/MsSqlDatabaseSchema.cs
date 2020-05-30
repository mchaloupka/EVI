using System;
using System.Collections.Generic;
using System.Data.Common;
using DatabaseSchemaReader;
using Slp.Evi.Common;
using Slp.Evi.Common.DatabaseConnection;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlDatabaseSchema
        : ISqlDatabaseSchema
    {
        private readonly Dictionary<string, MsSqlTable> _tableCache;

        private MsSqlDatabaseSchema(Dictionary<string, MsSqlTable> tableCache)
        {
            _tableCache = tableCache;
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Types.LiteralValueType DetectDefaultRdfType(string tableName, string columnName)
        {
            throw new NotImplementedException();
        }
    }
}
