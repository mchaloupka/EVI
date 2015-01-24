using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    /// <summary>
    /// IS NULL condition
    /// </summary>
    public class IsNullCondition : ICondition
    {
        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>The column.</value>
        public ISqlColumn Column { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IsNullCondition"/> class.
        /// </summary>
        /// <param name="sqlColumn">The SQL column.</param>
        public IsNullCondition(ISqlColumn sqlColumn)
        {
            Column = sqlColumn;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new IsNullCondition(Column);
        }
    }
}
