using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    /// <summary>
    /// SQL string statement
    /// </summary>
    public class SqlStatement : ISqlOriginalDbSource
    {
        /// <summary>
        /// Gets the SQL query.
        /// </summary>
        /// <value>The SQL query.</value>
        public string SqlQuery { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlStatement"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public SqlStatement(string query)
        {
            SqlQuery = query;
            _columns = new List<SqlTableColumn>();

            throw new NotImplementedException("SQL statements are not supported - not able to load schema");
        }

        /// <summary>
        /// The columns
        /// </summary>
        private readonly List<SqlTableColumn> _columns;

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>ISqlColumn.</returns>
        public ISqlColumn GetColumn(string columnName)
        {
            var col = _columns.FirstOrDefault(x => x.Name == columnName);

            if (col == null)
            {
                col = new SqlTableColumn(columnName, this, null); // TODO: Types for statements
                _columns.Add(col);
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
            get { return _columns.AsEnumerable(); }
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISqlSourceVisitor visitor, object data)
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
                _columns.Remove((SqlTableColumn)col);
            }
        }
    }
}
