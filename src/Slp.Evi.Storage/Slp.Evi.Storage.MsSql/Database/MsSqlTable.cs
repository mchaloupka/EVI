using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlTable
    {
        private readonly Dictionary<string, MsSqlColumn> _columns;

        public MsSqlTable(string tableName, IEnumerable<MsSqlColumn> columns)
        {
            Name = tableName;
            _columns = columns.ToDictionary(column => column.Name);
        }

        public string Name { get; }

        public static MsSqlTable CreateFromDatabase(DatabaseTable tableSchema)
        {
            var columns = tableSchema.Columns.Select(MsSqlColumn.CreateFromDatabase);
            return new MsSqlTable(tableSchema.Name, columns);
        }

        public LiteralValueType DetectDefaultRdfType(string columnName)
        {
            if (_columns.TryGetValue(columnName, out var column))
            {
                return column.DefaultRdfType;
            }
            else
            {
                throw new ArgumentException($"Column ${columnName} was not found in the table ${Name}");
            }
        }
    }
}
