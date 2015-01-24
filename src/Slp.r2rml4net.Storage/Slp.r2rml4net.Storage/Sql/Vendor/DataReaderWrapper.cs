using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using TCode.r2rml4net;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    /// <summary>
    /// Wrapper for standard SQL reader
    /// </summary>
    public class DataReaderWrapper : IQueryResultReader
    {
        /// <summary>
        /// The data reader
        /// </summary>
        private SqlDataReader _dataReader;

        /// <summary>
        /// The needs dispose action
        /// </summary>
        private readonly Func<bool> _needsDisposeAction;

        /// <summary>
        /// The dispose action
        /// </summary>
        private readonly Action _disposeAction;

        /// <summary>
        /// The current row
        /// </summary>
        private IQueryResultRow _currentRow;

        /// <summary>
        /// Gets the column name unquoted.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>Unquoted column name.</returns>
        public static string GetColumnNameUnquoted(string columnName) 
        {
            return DataReaderRow.GetColumnNameUnquoted(columnName);
        }

        /// <summary>
        /// Gets the table name unquoted.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <returns>Unquoted table name.</returns>
        public static string GetTableNameUnquoted(string tableName)
        {
            return DataReaderRow.GetColumnNameUnquoted(tableName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataReaderWrapper"/> class.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="needsDisposeAction">The needs dispose action.</param>
        /// <param name="disposeAction">The dispose action.</param>
        public DataReaderWrapper(SqlDataReader dataReader, Func<bool> needsDisposeAction, Action disposeAction)
        {
            _dataReader = dataReader;
            _needsDisposeAction = needsDisposeAction;
            _disposeAction = disposeAction;
            _currentRow = null;

            Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            if(_dataReader.HasRows)
            {
                FetchRow();
            }
        }

        /// <summary>
        /// Fetchs the row.
        /// </summary>
        private void FetchRow()
        {
            _currentRow = _dataReader.Read() ? DataReaderRow.Create(_dataReader) : null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(_dataReader != null)
            {
                _dataReader.Close();
                _dataReader.Dispose();
                _dataReader = null;
            }

            if (_needsDisposeAction())
                _disposeAction();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has next row.
        /// </summary>
        /// <value><c>true</c> if this instance has next row; otherwise, <c>false</c>.</value>
        public bool HasNextRow { get { return _currentRow != null; } }

        /// <summary>
        /// Reads the current row and moves to next one.
        /// </summary>
        /// <returns>Readed row, <c>null</c> if there is no row</returns>
        public IQueryResultRow Read()
        {
            var row = _currentRow;
            FetchRow();
            return row;
        }

        /// <summary>
        /// Wrapper for a readed row
        /// </summary>
        private class DataReaderRow : IQueryResultRow
        {
            /// <summary>
            /// The columns
            /// </summary>
            private readonly Dictionary<string, IQueryResultColumn> _columns;

            /// <summary>
            /// Initializes a new instance of the <see cref="DataReaderRow"/> class.
            /// </summary>
            /// <param name="columns">The columns.</param>
            private DataReaderRow(List<IQueryResultColumn> columns)
            {
                _columns = new Dictionary<string, IQueryResultColumn>();

                foreach (var col in columns)
                {
                    _columns.Add(col.Name, col);
                }
            }

            /// <summary>
            /// Gets the columns.
            /// </summary>
            /// <value>The columns.</value>
            public IEnumerable<IQueryResultColumn> Columns
            {
                get { return _columns.Select(x => x.Value); }
            }

            /// <summary>
            /// Creates the specified data reader.
            /// </summary>
            /// <param name="dataReader">The data reader.</param>
            /// <returns>IQueryResultRow.</returns>
            public static IQueryResultRow Create(SqlDataReader dataReader)
            {
                List<IQueryResultColumn> columns = new List<IQueryResultColumn>();
                var fieldCount = dataReader.VisibleFieldCount;

                for (int i = 0; i < fieldCount; i++)
                {
                    var name = dataReader.GetName(i);
                    var value = dataReader.GetValue(i);

                    columns.Add(new DataReaderColumn(EnsureColumnNameUndelimited(name), value));
                }

                return new DataReaderRow(columns);
            }

            /// <summary>
            /// The start delimiters
            /// </summary>
            private static readonly char[] StartDelimiters = new[] { '`', '\"', '[' };

            /// <summary>
            /// The end delimiters
            /// </summary>
            private static readonly char[] EndDelimiters = new[] { '`', '\"', ']' };

            /// <summary>
            /// The column name regex
            /// </summary>
            private static readonly Regex ColumnNameRegex = new Regex(@"^[\""`'\[](.+[^\""`'\]])[\""`'\]]$");

            /// <summary>
            /// Gets the column name unquoted.
            /// </summary>
            /// <param name="columnName">Name of the column.</param>
            /// <returns>Unquoted column name.</returns>
            public static string GetColumnNameUnquoted(string columnName)
            {
                return columnName.TrimStart(StartDelimiters).TrimEnd(EndDelimiters);
            }

            /// <summary>
            /// Delimits the identifier.
            /// </summary>
            /// <param name="identifier">The identifier.</param>
            /// <returns>Delimited identifier.</returns>
            private static string DelimitIdentifier(string identifier)
            {
                if (MappingOptions.Current.UseDelimitedIdentifiers && !ColumnNameRegex.IsMatch(identifier))
                    return string.Format("{0}{1}{2}", MappingOptions.Current.SqlIdentifierLeftDelimiter, identifier, MappingOptions.Current.SqlIdentifierRightDelimiter);

                return identifier;
            }

            /// <summary>
            /// Ensures the column name undelimited.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <returns>Undelimited column name.</returns>
            private static string EnsureColumnNameUndelimited(string name)
            {
                return name.TrimStart(StartDelimiters).TrimEnd(EndDelimiters);
            }

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <param name="columnName">Name of the column.</param>
            /// <returns>The column.</returns>
            /// <exception cref="System.Exception">Asked for column that is not present</exception>
            public IQueryResultColumn GetColumn(string columnName)
            {
                var cName = EnsureColumnNameUndelimited(columnName);

                if (_columns.ContainsKey(cName))
                    return _columns[cName];
                else
                    throw new Exception("Asked for column that is not present");
            }
        }

        /// <summary>
        /// Wrapper for a column
        /// </summary>
        private class DataReaderColumn : IQueryResultColumn
        {
            /// <summary>
            /// The name
            /// </summary>
            private readonly string _name;

            /// <summary>
            /// The value
            /// </summary>
            private readonly object _value;

            /// <summary>
            /// Initializes a new instance of the <see cref="DataReaderColumn"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="value">The value.</param>
            public DataReaderColumn(string name, object value)
            {
                _name = name;

                if (value is DBNull)
                    _value = null;
                else
                    _value = value;
            }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get { return _name; } }

            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <value>The value.</value>
            public object Value { get { return _value; } }

            /// <summary>
            /// Gets the boolean value.
            /// </summary>
            /// <returns><c>true</c> if the value is true, <c>false</c> otherwise.</returns>
            /// <exception cref="System.Exception">Cannot convert value to boolean</exception>
            public bool GetBooleanValue()
            {
                if (_value is bool)
                {
                    return (bool)_value;
                }
                else if(_value is int)
                {
                    return ((int)_value) == 1;
                }
                else
                {
                    throw new Exception("Cannot convert value to boolean");
                }
            }
        }
    }
}
