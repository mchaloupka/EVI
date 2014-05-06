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
    public class DataReaderWrapper : IQueryResultReader
    {
        private SqlDataReader dataReader;
        private Func<bool> needsDisposeAction;
        private Action disposeAction;

        private IQueryResultRow currentRow;

        public static string GetColumnNameUnquoted(string col) 
        {
            return DataReaderRow.GetColumnNameUnquoted(col);
        }

        public static string GetTableNameUnquoted(string col)
        {
            return DataReaderRow.GetColumnNameUnquoted(col);
        }

        public DataReaderWrapper(SqlDataReader dataReader, Func<bool> needsDisposeAction, Action disposeAction)
        {
            this.dataReader = dataReader;
            this.needsDisposeAction = needsDisposeAction;
            this.disposeAction = disposeAction;
            this.currentRow = null;

            Init();
        }

        private void Init()
        {
            if(this.dataReader.HasRows)
            {
                FetchRow();
            }
        }

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

        public bool HasNextRow { get { return currentRow != null; } }

        public IQueryResultRow Read()
        {
            var row = this.currentRow;
            FetchRow();
            return row;
        }

        private class DataReaderRow : IQueryResultRow
        {
            private Dictionary<string, IQueryResultColumn> columns;
            private DataReaderRow(List<IQueryResultColumn> columns)
            {
                this.columns = new Dictionary<string, IQueryResultColumn>();

                foreach (var col in columns)
                {
                    this.columns.Add(col.Name, col);
                }
            }

            public IEnumerable<IQueryResultColumn> Columns
            {
                get { return columns.Select(x => x.Value); }
            }

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

            private static readonly char[] StartDelimiters = new[] { '`', '\"', '[' };
            private static readonly char[] EndDelimiters = new[] { '`', '\"', ']' };

            private static readonly Regex ColumnNameRegex = new Regex(@"^[\""`'\[](.+[^\""`'\]])[\""`'\]]$");

            public static string GetColumnNameUnquoted(string columnName)
            {
                return columnName.TrimStart(StartDelimiters).TrimEnd(EndDelimiters);
            }

            private static string DelimitIdentifier(string identifier)
            {
                if (MappingOptions.Current.UseDelimitedIdentifiers && !ColumnNameRegex.IsMatch(identifier))
                    return string.Format("{0}{1}{2}", MappingOptions.Current.SqlIdentifierLeftDelimiter, identifier, MappingOptions.Current.SqlIdentifierRightDelimiter);

                return identifier;
            }

            private static string EnsureColumnNameUndelimited(string name)
            {
                return name.TrimStart(StartDelimiters).TrimEnd(EndDelimiters);
            }

            public IQueryResultColumn GetColumn(string columnName)
            {
                var cName = EnsureColumnNameUndelimited(columnName);

                if (columns.ContainsKey(cName))
                    return columns[cName];
                else
                    throw new Exception("Asked for column that is not present");
            }


        }

        private class DataReaderColumn : IQueryResultColumn
        {
            private string name;
            private object value;

            public DataReaderColumn(string name, object value)
            {
                this.name = name;

                if (value is System.DBNull)
                    this.value = null;
                else
                    this.value = value;
            }

            public string Name { get { return name; } }

            public object Value { get { return value; } }

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
