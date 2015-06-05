using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Slp.r2rml4net.Storage.Sparql.Old.Operator
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
        private readonly List<ISparqlQuery> _unioned;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionOp"/> class.
        /// </summary>
        public UnionOp()
        {
            _unioned = new List<ISparqlQuery>();
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
                    _unioned.Add(inner);
                }
            }
            else
            {
                _unioned.Add(sparqlQuery);
            }
        }

        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            return _unioned.AsEnumerable();
        }


        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="newQuery">The new query.</param>
        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            var i = _unioned.IndexOf(originalQuery);

            if (i > -1)
            {
                if(newQuery is NoSolutionOp)
                {
                    _unioned.RemoveAt(i);
                }
                else
                {
                    _unioned[i] = newQuery;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("UNION({0})", string.Join(",", _unioned));
        }


        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        public ISparqlQuery FinalizeAfterTransform()
        {
            if (_unioned.Count == 0)
            {
                return new NoSolutionOp();
            }
            else if (_unioned.Count == 1)
            {
                return _unioned[0];
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
