using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// The disjunction of conditions
    /// </summary>
    public class ConjunctionCondition
        : IFilterCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConjunctionCondition"/> class.
        /// </summary>
        /// <param name="conditions">The inner conditions.</param>
        public ConjunctionCondition(IEnumerable<IFilterCondition> conditions)
        {
            InnerConditions = conditions.ToArray();
            UsedCalculusVariables = InnerConditions.SelectMany(x => x.UsedCalculusVariables).Distinct().ToList();
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IFilterConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the inner conditions.
        /// </summary>
        /// <value>The inner conditions.</value>
        public IEnumerable<IFilterCondition> InnerConditions { get; }

        /// <summary>
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }
    }
}
