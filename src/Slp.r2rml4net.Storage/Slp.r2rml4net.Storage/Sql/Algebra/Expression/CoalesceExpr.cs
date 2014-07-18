using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private List<IExpression> expressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoalesceExpr"/> class.
        /// </summary>
        public CoalesceExpr()
        {
            this.expressions = new List<IExpression>();
        }

        /// <summary>
        /// Adds the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public void AddExpression(IExpression expression)
        {
            this.expressions.Add(expression);
        }

        /// <summary>
        /// Gets the expressions.
        /// </summary>
        /// <value>The expressions.</value>
        public IEnumerable<IExpression> Expressions { get { return expressions; } }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            var col = new CoalesceExpr();
            foreach (var expr in expressions)
            {
                col.expressions.Add((IExpression)expr.Clone());
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
            var index = expressions.IndexOf(oldExpr);

            if (index > -1)
                expressions[index] = newExpr;
        }

        /// <summary>
        /// Removes the expression.
        /// </summary>
        /// <param name="subExpr">The sub expression.</param>
        public void RemoveExpression(IExpression subExpr)
        {
            var index = expressions.IndexOf(subExpr);

            if (index > -1)
                expressions.RemoveAt(index);
        }
    }
}
