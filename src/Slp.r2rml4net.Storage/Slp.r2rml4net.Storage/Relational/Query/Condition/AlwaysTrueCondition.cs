using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Relational.Query.Condition
{
    /// <summary>
    /// The always true condition
    /// </summary>
    public class AlwaysTrueCondition 
        : ICondition
    {
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
    }
}