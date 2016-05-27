using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Visits <see cref="NegationExpression"/>
        /// </summary>
        /// <param name="negationExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(NegationExpression negationExpression, object data);

        /// <summary>
        /// Visits <see cref="ComparisonExpression"/>
        /// </summary>
        /// <param name="comparisonExpression">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ComparisonExpression comparisonExpression, object data);
    }
}
