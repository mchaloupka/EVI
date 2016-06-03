using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.Query.Expressions
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
            UsedCalculusVariables = _expressions.SelectMany(x => x.UsedCalculusVariables).Distinct().ToArray();
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

        /// <summary>
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }
    }
}