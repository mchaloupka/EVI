using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Expression representing negation
    /// </summary>
    public class NegationExpression
        : ISparqlCondition
    {
        /// <summary>
        /// Gets the inner condition.
        /// </summary>
        /// <value>The inner condition.</value>
        public ISparqlCondition InnerCondition { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NegationExpression"/> class.
        /// </summary>
        /// <param name="innerCondition">The inner condition.</param>
        public NegationExpression(ISparqlCondition innerCondition)
        {
            InnerCondition = innerCondition;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the needed variables to evaluate the expression.
        /// </summary>
        public IEnumerable<string> NeededVariables => InnerCondition.NeededVariables;
    }
}
