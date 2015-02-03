using System.Collections.Generic;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    /// <summary>
    /// COALESCE Expression.
    /// </summary>
    public class CoalesceExpr : IExpression
    {
        /// <summary>
        /// The expressions
        /// </summary>
        private readonly List<IExpression> _expressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoalesceExpr"/> class.
        /// </summary>
        /// <param name="sqlDataType">The SQL data type</param>
        public CoalesceExpr(DataType sqlDataType)
        {
            _expressions = new List<IExpression>();
            SqlType = sqlDataType;
        }

        /// <summary>
        /// Adds the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public void AddExpression(IExpression expression)
        {
            _expressions.Add(expression);
        }

        /// <summary>
        /// Gets the expressions.
        /// </summary>
        /// <value>The expressions.</value>
        public IEnumerable<IExpression> Expressions { get { return _expressions; } }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            var col = new CoalesceExpr(SqlType);
            foreach (var expr in _expressions)
            {
                col._expressions.Add((IExpression)expr.Clone());
            }
            return col;
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
        /// Replaces the expression.
        /// </summary>
        /// <param name="oldExpr">The old expression.</param>
        /// <param name="newExpr">The new expression.</param>
        public void ReplaceExpression(IExpression oldExpr, IExpression newExpr)
        {
            var index = _expressions.IndexOf(oldExpr);

            if (index > -1)
                _expressions[index] = newExpr;
        }

        /// <summary>
        /// Removes the expression.
        /// </summary>
        /// <param name="subExpr">The sub expression.</param>
        public void RemoveExpression(IExpression subExpr)
        {
            var index = _expressions.IndexOf(subExpr);

            if (index > -1)
                _expressions.RemoveAt(index);
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; private set; }
    }
}
