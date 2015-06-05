using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Old;
using Slp.r2rml4net.Storage.Sparql.Old.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    /// <summary>
    /// Select into union optimization
    /// </summary>
    public class SelectIntoUnionOptimizer : ISparqlAlgebraOptimizer
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            if (algebra is SelectOp)
            {
                return ProcessSelect((SelectOp)algebra, context).FinalizeAfterTransform();
            }
            else
            {
                var innerQueries = algebra.GetInnerQueries().ToList();

                foreach (var query in innerQueries)
                {
                    var processed = ProcessAlgebra(query, context);

                    if (processed != query)
                    {
                        algebra.ReplaceInnerQuery(query, processed);
                    }
                }

                return algebra.FinalizeAfterTransform();
            }
        }

        /// <summary>
        /// Processes the select.
        /// </summary>
        /// <param name="selectOp">The select operator.</param>
        /// <param name="data">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public ISparqlQuery ProcessSelect(SelectOp selectOp, object data)
        {
            var context = (QueryContext)data;

            var inner = ProcessAlgebra(selectOp.InnerQuery, context);

            if (inner != selectOp.InnerQuery)
                selectOp.ReplaceInnerQuery(selectOp.InnerQuery, inner);

            if (inner is UnionOp && IsProjectionOnly(selectOp))
            {
                UnionOp union = new UnionOp();

                foreach (var source in ((UnionOp)inner).GetInnerQueries())
                {
                    var projectedSource = CreateProjection(selectOp, source, context);
                    union.AddToUnion(projectedSource);
                }

                return union;
            }
            else
            {

                return selectOp;
            }
        }

        /// <summary>
        /// Creates the projection.
        /// </summary>
        /// <param name="selectOp">The select operator.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The select operator.</returns>
        private SelectOp CreateProjection(SelectOp selectOp, ISparqlQuery source, QueryContext context)
        {
            if (selectOp.IsSelectAll)
                return new SelectOp(source);
            else
                return new SelectOp(source, selectOp.Variables);
        }

        /// <summary>
        /// Determines whether the specified select op is projection only.
        /// </summary>
        /// <param name="selectOp">The select operator.</param>
        /// <returns><c>true</c> if the specified select op is projection only; otherwise, <c>false</c>.</returns>
        private bool IsProjectionOnly(SelectOp selectOp)
        {
            if (selectOp.IsSelectAll)
                return true;
            else if (selectOp.Variables.Any(x => x.IsAggregate))
                return false;
            else
                return true;
        }
    }
}
