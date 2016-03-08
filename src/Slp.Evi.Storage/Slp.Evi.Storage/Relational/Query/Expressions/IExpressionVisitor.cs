using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query.Expressions
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
    }
}
