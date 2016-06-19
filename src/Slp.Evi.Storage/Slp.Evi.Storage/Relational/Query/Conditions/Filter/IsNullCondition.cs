using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// The  IS  NULL Condition.
    /// </summary>
    public class IsNullCondition
        : IFilterCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsNullCondition"/> class.
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public IsNullCondition(ICalculusVariable calculusVariable)
        {
            Variable = calculusVariable;
            UsedCalculusVariables = new ICalculusVariable[] {Variable};
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>The variable.</value>
        public ICalculusVariable Variable { get; }

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
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }
    }
}
