using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.SqlQuery
{
    public class StaticDataReader : IQueryResultReader
    {
        private IEnumerable<StaticDataReaderRow> rows;
        private IEnumerator<StaticDataReaderRow> enumerator;

        public StaticDataReader(params StaticDataReaderRow[] rows)
            :this((IEnumerable<StaticDataReaderRow>)rows)
        { }

        public StaticDataReader(IEnumerable<StaticDataReaderRow> rows)
        {
            this.rows = rows;
            this.enumerator = this.rows.GetEnumerator();
            this.HasNextRow = this.enumerator.MoveNext();
        }

        public bool HasNextRow { get; private set; }

        public IQueryResultRow Read()
        {
            var current = this.enumerator.Current;
            this.HasNextRow = this.enumerator.MoveNext();
            return current;
        }

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if(disposing)
            {
                this.enumerator.Dispose();
            }

            disposed = true;
        }

        ~StaticDataReader()
        {
            Dispose(false);
        }
    }

    public class StaticDataReaderRow : IQueryResultRow
    {
        public StaticDataReaderRow(params StaticDataReaderColumn[] columns)
            : this((IEnumerable<StaticDataReaderColumn>)columns)
        { }

        public StaticDataReaderRow(IEnumerable<StaticDataReaderColumn> columns)
        {
            this.columns = new Dictionary<string, StaticDataReaderColumn>();

            foreach (var item in columns)
            {
                this.columns.Add(item.Name, item);
            }
        }

        private Dictionary<string, StaticDataReaderColumn> columns;

        public IEnumerable<IQueryResultColumn> Columns
        {
            get { return this.columns.Values; }
        }

        public IQueryResultColumn GetColumn(string columnName)
        {
            if (this.columns.ContainsKey(columnName))
                return this.columns[columnName];
            else
                return null;
        }
    }

    public class StaticDataReaderColumn : IQueryResultColumn
    {
        public StaticDataReaderColumn(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; private set; }

        public object Value { get; private set; }

        public bool GetBooleanValue()
        {
            if (this.Value is bool)
                return (bool)this.Value;
            else
                return false;
        }
    }


}
