using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using Slp.Evi.Storage.Core.Database;

namespace Slp.Evi.Storage.MySql.Reader
{
    public class MySqlReaderRow
        : ISqlResultRow
    {
        private readonly Dictionary<string, MySqlReaderColumn> _columns;

        private MySqlReaderRow(IEnumerable<MySqlReaderColumn> columns)
        {
            _columns = columns.ToDictionary(x => x.Name);
        }

        public static MySqlReaderRow Create(MySqlDataReader reader)
        {
            var columns = new List<MySqlReaderColumn>();
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

                    if (fieldType == typeof(int) || fieldType == typeof(long))
                    {
                        variableValue = VariableValue.NewIntVariableValue(reader.GetInt32(i));
                    }
                    else if (fieldType == typeof(string))
                    {
                        variableValue = VariableValue.NewStringVariableValue(reader.GetString(i));
                    }
                    else if (fieldType == typeof(double) || fieldType == typeof(float))
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

                columns.Add(new MySqlReaderColumn(name, variableValue));
            }

            return new MySqlReaderRow(columns);
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