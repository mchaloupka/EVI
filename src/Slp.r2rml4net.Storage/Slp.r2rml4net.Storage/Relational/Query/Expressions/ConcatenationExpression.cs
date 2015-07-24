using System.Collections.Generic;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;

namespace Slp.r2rml4net.Storage.Relational.Query.Expressions
{
    /// <summary>
    /// The concatenation expression
    /// </summary>
    public class ConcatenationExpression 
        : IExpression
    {
        /// <summary>
        /// The concatenated expressions
        /// </summary>
        private readonly List<IExpression> _expressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatenationExpression"/> class.
        /// </summary>
        /// <param name="innerExpressions">The inner expressions.</param>
        /// <param name="sqlTypeForString">The SQL type for string.</param>
        public ConcatenationExpression(List<IExpression> innerExpressions, DataType sqlTypeForString)
        {
            _expressions = innerExpressions;
            SqlType = sqlTypeForString;
        }

        /// <summary>
        /// The concatenated expressions
        /// </summary>
        public IEnumerable<IExpression> InnerExpressions
        {
            get { return _expressions; }
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
        public DataType SqlType { get; private set; }
    }
}