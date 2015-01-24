namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// Column containing an expression
    /// </summary>
    public class SqlExpressionColumn : ISqlColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlExpressionColumn"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="source">The source.</param>
        public SqlExpressionColumn(IExpression expression, ISqlSource source)
        {
            Expression = expression;
            Source = source;
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
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public IExpression Expression { get; set; }
    }
}
