using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    /// <summary>
    /// Distinct operator.
    /// </summary>
    public class DistinctOp : ISparqlQueryModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctOp"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        public DistinctOp(ISparqlQuery innerQuery)
        {
            InnerQuery = innerQuery;
        }

        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        public ISparqlQuery InnerQuery { get; private set; }

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
            if (originalQuery == InnerQuery)
                InnerQuery = newQuery;
        }

        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        public ISparqlQuery FinalizeAfterTransform()
        {
            if (InnerQuery is NoSolutionOp || InnerQuery is OneEmptySolutionOp)
                return InnerQuery;
            else
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
    }
}
