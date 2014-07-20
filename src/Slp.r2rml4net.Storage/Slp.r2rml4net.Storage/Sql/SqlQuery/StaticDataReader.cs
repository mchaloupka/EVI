using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.SqlQuery
{
    /// <summary>
    /// Static data reader
    /// </summary>
    public class StaticDataReader : IQueryResultReader
    {
        /// <summary>
        /// The rows
        /// </summary>
        private IEnumerable<StaticDataReaderRow> rows;

        /// <summary>
        /// The enumerator
        /// </summary>
        private IEnumerator<StaticDataReaderRow> enumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataReader"/> class.
        /// </summary>
        /// <param name="rows">The rows.</param>
        public StaticDataReader(params StaticDataReaderRow[] rows)
            :this((IEnumerable<StaticDataReaderRow>)rows)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataReader"/> class.
        /// </summary>
        /// <param name="rows">The rows.</param>
        public StaticDataReader(IEnumerable<StaticDataReaderRow> rows)
        {
            this.rows = rows;
            this.enumerator = this.rows.GetEnumerator();
            this.HasNextRow = this.enumerator.MoveNext();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has next row.
        /// </summary>
        /// <value><c>true</c> if this instance has next row; otherwise, <c>false</c>.</value>
        public bool HasNextRow { get; private set; }

        /// <summary>
        /// Reads the current row and moves to next one.
        /// </summary>
        /// <returns>Readed row, <c>null</c> if there is no row</returns>
        public IQueryResultRow Read()
        {
            var current = this.enumerator.Current;
            this.HasNextRow = this.enumerator.MoveNext();
            return current;
        }

        /// <summary>
        /// The disposed
        /// </summary>
        bool disposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

        /// <summary>
        /// Finalizes an instance of the <see cref="StaticDataReader"/> class.
        /// </summary>
        ~StaticDataReader()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Static data reader row
    /// </summary>
    public class StaticDataReaderRow : IQueryResultRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataReaderRow"/> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        public StaticDataReaderRow(params StaticDataReaderColumn[] columns)
            : this((IEnumerable<StaticDataReaderColumn>)columns)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataReaderRow"/> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        public StaticDataReaderRow(IEnumerable<StaticDataReaderColumn> columns)
        {
            this.columns = new Dictionary<string, StaticDataReaderColumn>();

            foreach (var item in columns)
            {
                this.columns.Add(item.Name, item);
            }
        }

        /// <summary>
        /// The columns
        /// </summary>
        private Dictionary<string, StaticDataReaderColumn> columns;

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public IEnumerable<IQueryResultColumn> Columns
        {
            get { return this.columns.Values; }
        }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>IQueryResultColumn.</returns>
        public IQueryResultColumn GetColumn(string columnName)
        {
            if (this.columns.ContainsKey(columnName))
                return this.columns[columnName];
            else
                return null;
        }
    }

    /// <summary>
    /// Static data reader column
    /// </summary>
    public class StaticDataReaderColumn : IQueryResultColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDataReaderColumn"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public StaticDataReaderColumn(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the boolean value.
        /// </summary>
        /// <returns><c>true</c> if the value is true, <c>false</c> otherwise.</returns>
        public bool GetBooleanValue()
        {
            if (this.Value is bool)
                return (bool)this.Value;
            else
                return false;
        }
    }


}
