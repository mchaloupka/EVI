using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Sparql.Old.Operator
{
    /// <summary>
    /// Slice operator.
    /// </summary>
    public class SliceOp : ISparqlQueryModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SliceOp"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        public SliceOp(ISparqlQuery innerQuery)
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
            if (InnerQuery is NoSolutionOp)
                return InnerQuery;
            else if (!Offset.HasValue && !Limit.HasValue)
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

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>The offset.</value>
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        /// <value>The limit.</value>
        public int? Limit { get; set; }
    }
}
