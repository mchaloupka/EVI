namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// ORDER BY comparator
    /// </summary>
    public class SqlOrderByComparator
    {
        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public IExpression Expression { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SqlOrderByComparator"/> is descending.
        /// </summary>
        /// <value><c>true</c> if descending; otherwise, <c>false</c>.</value>
        public bool Descending { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlOrderByComparator"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        public SqlOrderByComparator(IExpression expression, bool descending)
        {
            Expression = expression;
            Descending = descending;
        }

    }
}
