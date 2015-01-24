using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private readonly List<IExpression> _parts;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatenationExpr"/> class.
        /// </summary>
        /// <param name="parts">The parts.</param>
        public ConcatenationExpr(IEnumerable<IExpression> parts)
        {
            _parts = parts.ToList();
        }

        /// <summary>
        /// Gets the parts.
        /// </summary>
        /// <value>The parts.</value>
        public IEnumerable<IExpression> Parts { get { return _parts; } }

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
            var newParts = _parts.Select(x => (IExpression)x.Clone());

            return new ConcatenationExpr(newParts);
        }

        /// <summary>
        /// Replaces the part.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="newPart">The new part.</param>
        public void ReplacePart(IExpression part, IExpression newPart)
        {
            int index = _parts.IndexOf(part);

            if (index > -1)
                _parts[index] = newPart;
        }
    }
}
