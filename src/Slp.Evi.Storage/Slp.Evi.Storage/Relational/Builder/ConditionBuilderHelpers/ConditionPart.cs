using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// Represents the condition for processing. It contains not only the
    /// main condition, but also the condition checking whether the condition
    /// does not produce an error.
    /// </summary>
    public class ConditionPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionPart"/> class.
        /// </summary>
        /// <param name="isNotErrorCondition">The condition when the main condition does not produce an error.</param>
        /// <param name="mainCondition">The main condition.</param>
        public ConditionPart(IFilterCondition isNotErrorCondition, IFilterCondition mainCondition)
        {
            IsNotErrorCondition = isNotErrorCondition;
            MainCondition = mainCondition;
        }

        /// <summary>
        /// Gets the condition when the main condition does not produce an error.
        /// </summary>
        public IFilterCondition IsNotErrorCondition { get; }

        /// <summary>
        /// Gets the main condition.
        /// </summary>
        public IFilterCondition MainCondition { get; }
    }
}
