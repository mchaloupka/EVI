using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Expression
{
    /// <summary>
    /// Variable expression.
    /// </summary>
    public class VariableT : ISparqlQueryExpression
    {
        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>The variable.</value>
        public string Variable { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableT"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public VariableT(string variable)
        {
            this.Variable = variable;
        }
    }
}
