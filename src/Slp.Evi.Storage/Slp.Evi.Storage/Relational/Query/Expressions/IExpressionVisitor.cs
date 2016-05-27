using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Relational.Query.Expressions
{
    /// <summary>
    /// The expression visitor
    /// </summary>
    public interface IExpressionVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="columnExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ColumnExpression columnExpression, object data);

        /// <summary>
        /// Visits <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="concatenationExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ConcatenationExpression concatenationExpression, object data);

        /// <summary>
        /// Visits <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="constantExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ConstantExpression constantExpression, object data);

        /// <summary>
        /// Visits <see cref="CaseExpression"/>
        /// </summary>
        /// <param name="caseExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(CaseExpression caseExpression, object data);

        /// <summary>
        /// Visits <see cref="CoalesceExpression"/>
        /// </summary>
        /// <param name="coalesceExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(CoalesceExpression coalesceExpression, object data);
    }
}
