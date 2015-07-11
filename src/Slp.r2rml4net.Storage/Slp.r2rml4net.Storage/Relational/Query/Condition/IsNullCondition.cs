using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Condition
{
    /// <summary>
    /// The  IS  NULL Condition.
    /// </summary>
    public class IsNullCondition
        : ICondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsNullCondition"/> class.
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public IsNullCondition(ICalculusVariable calculusVariable)
        {
            this.Variable = calculusVariable;
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>The variable.</value>
        public ICalculusVariable Variable { get; private set; }
    }
}
