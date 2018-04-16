using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<string> NeededVariables { get; }
    }
}
