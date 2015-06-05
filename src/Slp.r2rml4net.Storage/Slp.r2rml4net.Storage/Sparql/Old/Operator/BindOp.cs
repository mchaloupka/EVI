using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Sparql.Old.Operator
{
    /// <summary>
    /// Bind operator.
    /// </summary>
    public class BindOp : ISparqlQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindOp"/> class.
        /// </summary>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="innerQuery">The inner query.</param>
        public BindOp(string varName, ISparqlQueryExpression expression, ISparqlQuery innerQuery)
        {
            VariableName = varName;
            Expression = expression;
            InnerQuery = innerQuery;
        }

        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield return InnerQuery;
        }

        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="newQuery">The new query.</param>
        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            if (InnerQuery == originalQuery)
                InnerQuery = newQuery;
        }

        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        public ISparqlQuery FinalizeAfterTransform()
        {
            return this;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public ISparqlQueryExpression Expression { get; set; }

        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        public ISparqlQuery InnerQuery { get; private set; }
    }
}
