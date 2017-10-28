using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.Query.Expressions
{
    /// <summary>
    /// Expression representing COALESCE expression
    /// </summary>
    public class CoalesceExpression
        : IExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoalesceExpression"/> class.
        /// </summary>
        /// <param name="innerExpressions">The inner expressions.</param>
        public CoalesceExpression(IEnumerable<IExpression> innerExpressions)
        {
            InnerExpressions = innerExpressions.ToArray();
            SqlType = innerExpressions.First().SqlType; // TODO: Better find SQL type
            UsedCalculusVariables = innerExpressions.SelectMany(x => x.UsedCalculusVariables).Distinct().ToArray();
        }

        /// <summary>
        /// Gets the inner expressions.
        /// </summary>
        /// <value>The inner expressions.</value>
        public IExpression[] InnerExpressions { get; private set; }

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
        public DataType SqlType { get; }

        /// <summary>
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }

        /// <inheritdoc />
        public bool HasAlwaysTheSameValue => InnerExpressions.All(x => x.HasAlwaysTheSameValue);
    }
}
