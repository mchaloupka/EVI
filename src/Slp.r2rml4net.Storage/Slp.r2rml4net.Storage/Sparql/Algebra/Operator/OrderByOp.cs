using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    /// <summary>
    /// Order by operator.
    /// </summary>
    public class OrderByOp : ISparqlQueryModifier
    {
        /// <summary>
        /// The orderings
        /// </summary>
        private List<OrderByComparator> orderings;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByOp"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        public OrderByOp(ISparqlQuery innerQuery)
        {
            this.InnerQuery = innerQuery;
            this.orderings = new List<OrderByComparator>();
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
        /// Adds the ordering.
        /// </summary>
        /// <param name="sparqlQueryExpression">The sparql query expression.</param>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        public void AddOrdering(ISparqlQueryExpression sparqlQueryExpression, bool descending)
        {
            this.orderings.Add(new OrderByComparator(sparqlQueryExpression, descending));
        }

        /// <summary>
        /// Gets the orderings.
        /// </summary>
        /// <value>The orderings.</value>
        public IEnumerable<OrderByComparator> Orderings { get { return orderings; } }
    }

    /// <summary>
    /// Order by comparator.
    /// </summary>
    public class OrderByComparator
    {
        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public ISparqlQueryExpression Expression { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="OrderByComparator"/> is descending.
        /// </summary>
        /// <value><c>true</c> if descending; otherwise, <c>false</c>.</value>
        public bool Descending { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByComparator"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        public OrderByComparator(ISparqlQueryExpression expression, bool descending)
        {
            this.Expression = expression;
            this.Descending = descending;
        }

    }
}
