using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;

namespace Slp.r2rml4net.Storage.Relational.Query.Expressions
{
    /// <summary>
    /// Column expression
    /// </summary>
    public class ColumnExpression 
        : IExpression
    {
        /// <summary>
        /// Gets the calculus variable.
        /// </summary>
        /// <value>The calculus variable.</value>
        public ICalculusVariable CalculusVariable { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is URI.
        /// </summary>
        /// <value><c>true</c> if this instance is URI; otherwise, <c>false</c>.</value>
        public bool IsUri { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnExpression"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="calculusVariable">The calculus variable.</param>
        /// <param name="isUri">if set to <c>true</c> [is URI].</param>
        public ColumnExpression(QueryContext context, ICalculusVariable calculusVariable, bool isUri)
        {
            CalculusVariable = calculusVariable;
            IsUri = isUri;
        }

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
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType
        {
            get { return CalculusVariable.SqlType; }
        }
    }
}