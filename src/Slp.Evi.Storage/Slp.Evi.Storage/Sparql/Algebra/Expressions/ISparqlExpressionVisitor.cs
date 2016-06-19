using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Visitor interface for SPARQL expressions
    /// </summary>
    public interface ISparqlExpressionVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="IsBoundExpression"/>
        /// </summary>
        /// <param name="isBoundExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(IsBoundExpression isBoundExpression, object data);

        /// <summary>
        /// Visits <see cref="BooleanTrueExpression"/>
        /// </summary>
        /// <param name="booleanTrueExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(BooleanTrueExpression booleanTrueExpression, object data);

        /// <summary>
        /// Visits <see cref="BooleanFalseExpression"/>
        /// </summary>
        /// <param name="booleanFalseExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(BooleanFalseExpression booleanFalseExpression, object data);

        /// <summary>
        /// Visits <see cref="NegationExpression"/>
        /// </summary>
        /// <param name="negationExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(NegationExpression negationExpression, object data);

        /// <summary>
        /// Visits <see cref="VariableExpression"/>
        /// </summary>
        /// <param name="variableExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(VariableExpression variableExpression, object data);

        /// <summary>
        /// Visits <see cref="ConjunctionExpression"/>
        /// </summary>
        /// <param name="conjunctionExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ConjunctionExpression conjunctionExpression, object data);

        /// <summary>
        /// Visits <see cref="ComparisonExpression"/>
        /// </summary>
        /// <param name="comparisonExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ComparisonExpression comparisonExpression, object data);

        /// <summary>
        /// Visits <see cref="NodeExpression"/>
        /// </summary>
        /// <param name="nodeExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(NodeExpression nodeExpression, object data);

        /// <summary>
        /// Visits <see cref="DisjunctionExpression"/>
        /// </summary>
        /// <param name="disjunctionExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(DisjunctionExpression disjunctionExpression, object data);
    }
}
