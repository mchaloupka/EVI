using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    /// <summary>
    /// Visitor for ICondition
    /// </summary>
    public interface IConditionVisitor : IVisitor
    {
        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(AlwaysFalseCondition condition, object data);

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(AlwaysTrueCondition condition, object data);

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(AndCondition condition, object data);

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(OrCondition condition, object data);

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(EqualsCondition condition, object data);

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(IsNullCondition condition, object data);

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(NotCondition condition, object data);
    }
}
