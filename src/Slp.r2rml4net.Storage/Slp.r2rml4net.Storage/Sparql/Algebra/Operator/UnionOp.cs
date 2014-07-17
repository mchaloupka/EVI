using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    /// <summary>
    /// Union operator.
    /// </summary>
    [DebuggerDisplay("UNION(Count = {unioned.Count})")]
    public class UnionOp : ISparqlQueryPart
    {
        /// <summary>
        /// The unioned queries.
        /// </summary>
        private List<ISparqlQuery> unioned;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionOp"/> class.
        /// </summary>
        public UnionOp()
        {
            unioned = new List<ISparqlQuery>();
        }

        /// <summary>
        /// Adds to union.
        /// </summary>
        /// <param name="sparqlQuery">The sparql query.</param>
        public void AddToUnion(ISparqlQuery sparqlQuery)
        {
            if(sparqlQuery is UnionOp)
            {
                foreach (var inner in ((UnionOp)sparqlQuery).GetInnerQueries())
                {
                    unioned.Add(inner);
                }
            }
            else
            {
                unioned.Add(sparqlQuery);
            }
        }

        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            return unioned.AsEnumerable();
        }


        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="newQuery">The new query.</param>
        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            var i = unioned.IndexOf(originalQuery);

            if (i > -1)
            {
                if(newQuery is NoSolutionOp)
                {
                    unioned.RemoveAt(i);
                }
                else
                {
                    unioned[i] = newQuery;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("UNION({0})", string.Join(",", unioned));
        }


        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        public ISparqlQuery FinalizeAfterTransform()
        {
            if (unioned.Count == 0)
            {
                return new NoSolutionOp();
            }
            else if (unioned.Count == 1)
            {
                return unioned[0];
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
