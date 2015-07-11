using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Condition
{
    /// <summary>
    /// Class NegationCondition.
    /// </summary>
    public class NegationCondition
        : ICondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NegationCondition"/> class.
        /// </summary>
        /// <param name="condition">The inner condition.</param>
        public NegationCondition(ICondition condition)
        {
            InnerCondition = condition;
        }

        /// <summary>
        /// Gets the inner condition.
        /// </summary>
        /// <value>The inner condition.</value>
        public ICondition InnerCondition { get; private set; }
    }
}
