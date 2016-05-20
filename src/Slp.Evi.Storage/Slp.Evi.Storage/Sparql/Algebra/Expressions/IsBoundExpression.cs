using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// The expression representing <c>bound</c> operator
    /// </summary>
    public class IsBoundExpression
        : ISparqlCondition
    {
        public IsBoundExpression(string variable)
        {
            this.Variable = variable;
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        public string Variable { get; private set; }

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
    }
}
