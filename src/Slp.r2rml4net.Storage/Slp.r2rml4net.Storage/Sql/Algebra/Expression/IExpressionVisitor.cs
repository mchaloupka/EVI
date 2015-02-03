using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    /// <summary>
    /// Visitor for IExpression
    /// </summary>
    public interface IExpressionVisitor : IVisitor
    {
        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(ColumnExpr expression, object data);

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(ConstantExpr expression, object data);

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(ConcatenationExpr expression, object data);

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="nullExpr">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(NullExpr nullExpr, object data);

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="coalesceExpr">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(CoalesceExpr coalesceExpr, object data);

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="caseExpr">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(CaseExpr caseExpr, object data);
    }
}
