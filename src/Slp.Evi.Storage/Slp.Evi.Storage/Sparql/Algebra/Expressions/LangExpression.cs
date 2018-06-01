using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Represents the lang expression
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.ISparqlExpression" />
    public class LangExpression
        : ISparqlExpression
    {
        /// <summary>
        /// Gets the inner sparql expression.
        /// </summary>
        public ISparqlExpression SparqlExpression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LangExpression"/> class.
        /// </summary>
        /// <param name="sparqlExpression">The inner sparql expression.</param>
        public LangExpression(ISparqlExpression sparqlExpression)
        {
            SparqlExpression = sparqlExpression;
            NeededVariables = SparqlExpression.NeededVariables;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <inheritdoc />
        public IEnumerable<string> NeededVariables { get; }
    }
}
