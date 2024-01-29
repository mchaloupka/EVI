using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Slp.Evi.Storage.Core.Database;

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

                VariableValue variableValue;
                if (reader.IsDBNull(i))
                {
                    variableValue = VariableValue.NullVariableValue;
                }
                else
                {
                    var fieldType = reader.GetFieldType(i);

                    if (fieldType == typeof(int))
                    {
                        variableValue = VariableValue.NewIntVariableValue(reader.GetInt32(i));
                    }
                    else if (fieldType == typeof(string))
                    {
                        variableValue = VariableValue.NewStringVariableValue(reader.GetString(i));
                    }
                    else if (fieldType == typeof(double))
                    {
                        variableValue = VariableValue.NewDoubleVariableValue(reader.GetDouble(i));
                    }
                    else if (fieldType == typeof(bool))
                    {
                        variableValue = VariableValue.NewBooleanVariableValue(reader.GetBoolean(i));
                    }
                    else if (fieldType == typeof(DateTime))
                    {
                        variableValue = VariableValue.NewDateTimeVariableValue(reader.GetDateTime(i));
                    }
                    else
                    {
                        throw new NotImplementedException($"The field type in the result is not yet supported: {fieldType}");
                    }
                }

                columns.Add(new MsSqlReaderColumn(name, variableValue));
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