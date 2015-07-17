using System;
using System.Collections.Generic;
using System.Diagnostics;
using Slp.r2rml4net.Storage.Query;

namespace Slp.r2rml4net.Storage.Relational.Query.Expression
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
        /// <param name="context">The context.</param>
        public ConcatenationExpression(List<IExpression> innerExpressions, QueryContext context)
        {
            _expressions = innerExpressions;
        }

        /// <summary>
        /// The concatenated expressions
        /// </summary>
        public List<IExpression> InnerExpressions
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
    }
}