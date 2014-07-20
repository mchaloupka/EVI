using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// UNION column.
    /// </summary>
    public class SqlUnionColumn : ISqlColumn
    {
        /// <summary>
        /// The original columns
        /// </summary>
        private List<ISqlColumn> originalColumns;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlUnionColumn"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public SqlUnionColumn(ISqlSource source)
        {
            this.Source = source;
            this.originalColumns = new List<ISqlColumn>();
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="column">The column.</param>
        public void AddColumn(ISqlColumn column)
        {
            this.originalColumns.Add(column);
        }

        /// <summary>
        /// Gets the original columns.
        /// </summary>
        /// <value>The original columns.</value>
        public IEnumerable<ISqlColumn> OriginalColumns { get { return this.originalColumns; } }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public ISqlSource Source { get; private set; }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="ccol">The column.</param>
        public void RemoveColumn(ISqlColumn ccol)
        {
            this.originalColumns.Remove(ccol);
        }
    }
}
