using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    /// <summary>
    /// Column expression.
    /// </summary>
    public class ColumnExpr : IExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnExpr"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isIriEscapedValue">if set to <c>true</c> [is iri escaped value].</param>
        public ColumnExpr(ISqlColumn column, bool isIriEscapedValue)
        {
            this.Column = column;
            this.IsIriEscapedValue = isIriEscapedValue;
        }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>The column.</value>
        public ISqlColumn Column { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is iri escaped value.
        /// </summary>
        /// <value><c>true</c> if this instance is iri escaped value; otherwise, <c>false</c>.</value>
        public bool IsIriEscapedValue { get; set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new ColumnExpr(this.Column, this.IsIriEscapedValue);
        }
    }
}
