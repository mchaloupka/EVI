using System;
using System.Collections.Generic;
using Slp.r2rml4net.Storage.Query;

namespace Slp.r2rml4net.Storage.Relational.Query.Expression
{
    /// <summary>
    /// The concatenation expression
    /// </summary>
    public class ConcatenationExpression : IExpression
    {
        /// <summary>
        /// The concatenated expressions
        /// </summary>
        private readonly List<IExpression> expressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatenationExpression"/> class.
        /// </summary>
        /// <param name="innerExpressions">The inner expressions.</param>
        /// <param name="context">The context.</param>
        public ConcatenationExpression(List<IExpression> innerExpressions, QueryContext context)
        {
            expressions = innerExpressions;
        }

        /// <summary>
        /// The concatenated expressions
        /// </summary>
        public List<IExpression> InnerExpressions
        {
            get { return expressions; }
        }
    }
}