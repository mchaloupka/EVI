using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private SqlDataReader dataReader;

        /// <summary>
        /// The needs dispose action
        /// </summary>
        private Func<bool> needsDisposeAction;

        /// <summary>
        /// The dispose action
        /// </summary>
        private Action disposeAction;

        /// <summary>
        /// The current row
        /// </summary>
        private IQueryResultRow currentRow;

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
            this.dataReader = dataReader;
            this.needsDisposeAction = needsDisposeAction;
            this.disposeAction = disposeAction;
            this.currentRow = null;

            Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            if(this.dataReader.HasRows)
            {
                FetchRow();
            }
        }

        /// <summary>
        /// Fetchs the row.
        /// </summary>
        private void FetchRow()
        {
            if (dataReader.Read())
            {
                this.currentRow = DataReaderRow.Create(dataReader);
            }
            else
            {
                this.currentRow = null;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(dataReader != null)
            {
                dataReader.Close();
                dataReader.Dispose();
                dataReader = null;
            }

            if (needsDisposeAction())
                disposeAction();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has next row.
        /// </summary>
        /// <value><c>true</c> if this instance has next row; otherwise, <c>false</c>.</value>
        public bool HasNextRow { get { return currentRow != null; } }

        /// <summary>
        /// Reads the current row and moves to next one.
        /// </summary>
        /// <returns>Readed row, <c>null</c> if there is no row</returns>
        public IQueryResultRow Read()
        {
            var row = this.currentRow;
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
            private Dictionary<string, IQueryResultColumn> columns;

            /// <summary>
            /// Initializes a new instance of the <see cref="DataReaderRow"/> class.
            /// </summary>
            /// <param name="columns">The columns.</param>
            private DataReaderRow(List<IQueryResultColumn> columns)
            {
                this.columns = new Dictionary<string, IQueryResultColumn>();

                foreach (var col in columns)
                {
                    this.columns.Add(col.Name, col);
                }
            }

            /// <summary>
            /// Gets the columns.
            /// </summary>
            /// <value>The columns.</value>
            public IEnumerable<IQueryResultColumn> Columns
            {
                get { return columns.Select(x => x.Value); }
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

                if (columns.ContainsKey(cName))
                    return columns[cName];
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
            private string name;

            /// <summary>
            /// The value
            /// </summary>
            private object value;

            /// <summary>
            /// Initializes a new instance of the <see cref="DataReaderColumn"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="value">The value.</param>
            public DataReaderColumn(string name, object value)
            {
                this.name = name;

                if (value is System.DBNull)
                    this.value = null;
                else
                    this.value = value;
            }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get { return name; } }

            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <value>The value.</value>
            public object Value { get { return value; } }

            /// <summary>
            /// Gets the boolean value.
            /// </summary>
            /// <returns><c>true</c> if the value is true, <c>false</c> otherwise.</returns>
            /// <exception cref="System.Exception">Cannot convert value to boolean</exception>
            public bool GetBooleanValue()
            {
                if (value is bool)
                {
                    return (bool)value;
                }
                else if(value is int)
                {
                    return ((int)value) == 1;
                }
                else
                {
                    throw new Exception("Cannot convert value to boolean");
                }
            }
        }
    }
}
