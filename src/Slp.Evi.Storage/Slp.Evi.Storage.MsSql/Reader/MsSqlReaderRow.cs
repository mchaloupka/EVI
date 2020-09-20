using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Slp.Evi.Database;

namespace Slp.Evi.Storage.MsSql.Reader
{
    public class MsSqlReaderRow
        : ISqlResultRow
    {
        private readonly Dictionary<string, MsSqlReaderColumn> _columns;

        private MsSqlReaderRow(IEnumerable<MsSqlReaderColumn> columns)
        {
            _columns = columns.ToDictionary(x => x.Name);
        }

        public static MsSqlReaderRow Create(SqlDataReader reader)
        {
            var columns = new List<MsSqlReaderColumn>();
            var fieldCount = reader.VisibleFieldCount;

            for (var i = 0; i < fieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetValue(i);

                columns.Add(new MsSqlReaderColumn(name, value));
            }

            return new MsSqlReaderRow(columns);
        }

        /// <inheritdoc />
        public ISqlResultColumn GetColumn(string columnName)
        {
            if (_columns.TryGetValue(columnName, out var value))
            {
                return value;
            }
            else
            {
                throw new ArgumentException($"Tried to retrieve a column {columnName} that is not available",
                    nameof(columnName));
            }
        }

        /// <inheritdoc />
        public IEnumerable<ISqlResultColumn> Columns => _columns.Values;
    }
}