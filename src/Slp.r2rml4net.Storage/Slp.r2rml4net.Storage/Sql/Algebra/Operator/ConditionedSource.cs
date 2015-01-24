namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// Conditioned source.
    /// </summary>
    public class ConditionedSource
    {
        /// <summary>
        /// Gets the condition.
        /// </summary>
        /// <value>The condition.</value>
        public ICondition Condition { get; private set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public ISqlSource Source { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionedSource"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="source">The source.</param>
        public ConditionedSource(ICondition condition, ISqlSource source)
        {
            Condition = condition;
            Source = source;
        }

        /// <summary>
        /// Replaces the condition.
        /// </summary>
        /// <param name="newCondition">The new condition.</param>
        public void ReplaceCondition(ICondition newCondition)
        {
            Condition = newCondition;
        }
    }
}
