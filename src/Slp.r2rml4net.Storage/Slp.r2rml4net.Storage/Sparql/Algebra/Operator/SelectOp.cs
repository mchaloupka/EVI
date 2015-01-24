using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    /// <summary>
    /// Select operator.
    /// </summary>
    [DebuggerDisplay("SELECT({InnerQuery})")]
    public class SelectOp : ISparqlQueryModifier
    {
        /// <summary>
        /// The variables
        /// </summary>
        private readonly List<SparqlVariable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOp"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        public SelectOp(ISparqlQuery innerQuery)
        {
            InnerQuery = innerQuery;
            _variables = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOp"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        /// <param name="variables">The variables.</param>
        public SelectOp(ISparqlQuery innerQuery, IEnumerable<SparqlVariable> variables)
        {
            InnerQuery = innerQuery;
            _variables = variables.ToList();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is select all.
        /// </summary>
        /// <value><c>true</c> if this instance is select all; otherwise, <c>false</c>.</value>
        public bool IsSelectAll { get { return _variables == null; } }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<SparqlVariable> Variables { get { return _variables; } }

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
            if (InnerQuery == originalQuery)
                InnerQuery = newQuery;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("SELECT({0})", InnerQuery);
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
