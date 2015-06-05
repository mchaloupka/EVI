using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Slp.r2rml4net.Storage.Sparql.Old.Operator
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
        private readonly List<ISparqlQuery> _joined;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinOp"/> class.
        /// </summary>
        public JoinOp()
        {
            _joined = new List<ISparqlQuery>();
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
                    _joined.Add(inner);
                }
            }
            else
            {
                _joined.Add(sparqlQuery);
            }
        }

        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            return _joined.AsEnumerable();
        }

        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="newQuery">The new query.</param>
        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            var i = _joined.IndexOf(originalQuery);

            if (i > -1)
            {
                if (newQuery is OneEmptySolutionOp)
                {
                    _joined.RemoveAt(i);
                }
                else
                {
                    _joined[i] = newQuery;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("JOIN({0})", string.Join(",", _joined));
        }

        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        public ISparqlQuery FinalizeAfterTransform()
        {
            if (_joined.OfType<NoSolutionOp>().Any())
            {
                return new NoSolutionOp();
            }
            else if (_joined.Count == 0)
            {
                return new OneEmptySolutionOp();
            }
            else if (_joined.Count == 1)
            {
                return _joined[0];
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
