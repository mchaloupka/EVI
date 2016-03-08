using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter
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
            this.Variable = calculusVariable;
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>The variable.</value>
        public ICalculusVariable Variable { get; private set; }

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
    }
}
