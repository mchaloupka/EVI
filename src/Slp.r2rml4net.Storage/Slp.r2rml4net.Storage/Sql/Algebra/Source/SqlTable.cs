using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    /// <summary>
    /// SQL table
    /// </summary>
    public class SqlTable : ISqlOriginalDbSource
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTable"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public SqlTable(string tableName)
        {
            this.TableName = tableName;
            this.columns = new List<SqlTableColumn>();
        }

        /// <summary>
        /// The columns
        /// </summary>
        private List<SqlTableColumn> columns;

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>ISqlColumn.</returns>
        public ISqlColumn GetColumn(string columnName)
        {
            var col = columns.Where(x => x.Name == columnName).FirstOrDefault();

            if (col == null)
            {
                col = new SqlTableColumn(columnName, this);
                columns.Add(col);
            }

            return col;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }


        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public IEnumerable<ISqlColumn> Columns
        {
            get { return columns.AsEnumerable(); }
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(Operator.ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="col">The col.</param>
        public void RemoveColumn(ISqlColumn col)
        {
            if (col is SqlTableColumn)
            {
                this.columns.Remove((SqlTableColumn)col);
            }
        }
    }
}
