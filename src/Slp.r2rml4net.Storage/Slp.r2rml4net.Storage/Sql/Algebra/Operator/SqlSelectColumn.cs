using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// SELECT column
    /// </summary>
    public class SqlSelectColumn : ISqlColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSelectColumn"/> class.
        /// </summary>
        /// <param name="originalColumn">The original column.</param>
        /// <param name="source">The source.</param>
        public SqlSelectColumn(ISqlColumn originalColumn, ISqlSource source)
        {
            this.OriginalColumn = originalColumn;
            this.Source = source;
            this.Name = null;
        }

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
        /// Gets the original column.
        /// </summary>
        /// <value>The original column.</value>
        public ISqlColumn OriginalColumn { get; private set; }
    }
}
