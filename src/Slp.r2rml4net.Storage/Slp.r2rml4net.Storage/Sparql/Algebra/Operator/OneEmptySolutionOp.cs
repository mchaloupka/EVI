using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    /// <summary>
    /// One empty solution operator.
    /// </summary>
    [DebuggerDisplay("Empty")]
    public class OneEmptySolutionOp : ISparqlQueryPart
    {
        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield break;
        }

        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="newQuery">The new query.</param>
        /// <exception cref="System.Exception">Should not be called, OneEmptySolutionOp has no subqueries</exception>
        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            throw new Exception("Should not be called, OneEmptySolutionOp has no subqueries");
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "Empty";
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
    }
}
