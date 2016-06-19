using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Representing a variable as an 
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.ISparqlExpression" />
    public class VariableExpression
        : ISparqlExpression
    {
        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>The variable.</value>
        public string Variable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableExpression"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public VariableExpression(string variable)
        {
            Variable = variable;
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
        public IEnumerable<string> NeededVariables => new string[] {Variable};
    }
}
