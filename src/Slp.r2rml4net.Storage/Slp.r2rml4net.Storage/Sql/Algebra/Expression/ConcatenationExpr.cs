using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    /// <summary>
    /// CONCAT expression.
    /// </summary>
    public class ConcatenationExpr : IExpression
    {
        /// <summary>
        /// The parts
        /// </summary>
        private List<IExpression> parts;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatenationExpr"/> class.
        /// </summary>
        /// <param name="parts">The parts.</param>
        public ConcatenationExpr(IEnumerable<IExpression> parts)
        {
            this.parts = parts.ToList();
        }

        /// <summary>
        /// Gets the parts.
        /// </summary>
        /// <value>The parts.</value>
        public IEnumerable<IExpression> Parts { get { return parts; } }

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
            var newParts = this.parts.Select(x => (IExpression)x.Clone());

            return new ConcatenationExpr(newParts);
        }

        /// <summary>
        /// Replaces the part.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="newPart">The new part.</param>
        public void ReplacePart(IExpression part, IExpression newPart)
        {
            int index = parts.IndexOf(part);

            if (index > -1)
                parts[index] = newPart;
        }
    }
}
