using System.Diagnostics;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// The always true condition
    /// </summary>
    public class AlwaysTrueCondition 
        : IFilterCondition
    {
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