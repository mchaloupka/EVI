using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    /// <summary>
    /// NOT Condition
    /// </summary>
    public class NotCondition : ICondition
    {
        /// <summary>
        /// Gets the inner condition.
        /// </summary>
        /// <value>The inner condition.</value>
        public ICondition InnerCondition { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotCondition"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public NotCondition(ICondition condition)
        {
            InnerCondition = condition;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new NotCondition((ICondition)InnerCondition.Clone());
        }
    }
}
