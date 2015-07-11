using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Source
{
    /// <summary>
    /// Model representing calculus representation of a query
    /// </summary>
    public class CalculusModel 
        : ICalculusSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculusModel"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="conditions">The conditions.</param>
        public CalculusModel(IEnumerable<ICalculusVariable> variables, IEnumerable<ICondition> conditions)
        {
            Variables = variables;
            Conditions = conditions;
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<ICalculusVariable> Variables { get; private set; }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>The conditions.</value>
        public IEnumerable<ICondition> Conditions { get; private set; }
    }
}
