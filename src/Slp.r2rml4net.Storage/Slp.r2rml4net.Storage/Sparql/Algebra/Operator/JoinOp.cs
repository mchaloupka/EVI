using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    /// <summary>
    /// Join operator.
    /// </summary>
    [DebuggerDisplay("JOIN(Count = {joined.Count})")]
    public class JoinOp : ISparqlQueryPart
    {
        /// <summary>
        /// The joined queries.
        /// </summary>
        private List<ISparqlQuery> joined;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinOp"/> class.
        /// </summary>
        public JoinOp()
        {
            joined = new List<ISparqlQuery>();
        }

        /// <summary>
        /// Adds to join.
        /// </summary>
        /// <param name="sparqlQuery">The sparql query.</param>
        public void AddToJoin(ISparqlQuery sparqlQuery)
        {
            if(sparqlQuery is JoinOp)
            {
                foreach (var inner in ((JoinOp)sparqlQuery).GetInnerQueries())
                {
                    joined.Add(inner);
                }
            }
            else
            {
                joined.Add(sparqlQuery);
            }
        }

        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            return joined.AsEnumerable();
        }

        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="newQuery">The new query.</param>
        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            var i = joined.IndexOf(originalQuery);

            if (i > -1)
            {
                if (newQuery is OneEmptySolutionOp)
                {
                    joined.RemoveAt(i);
                }
                else
                {
                    joined[i] = newQuery;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("JOIN({0})", string.Join(",", joined));
        }

        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        public ISparqlQuery FinalizeAfterTransform()
        {
            if (joined.OfType<NoSolutionOp>().Any())
            {
                return new NoSolutionOp();
            }
            else if (joined.Count == 0)
            {
                return new OneEmptySolutionOp();
            }
            else if (joined.Count == 1)
            {
                return joined[0];
            }
            else
            {
                return this;
            }
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
