using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    public class SelectIntoUnionOptimizer : ISparqlAlgebraOptimizer
    {
        public ISparqlQuery ProcessAlgebra(Sparql.Algebra.ISparqlQuery algebra, Query.QueryContext context)
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

        private SelectOp CreateProjection(SelectOp selectOp, ISparqlQuery source, QueryContext vd)
        {
            if (selectOp.IsSelectAll)
                return new SelectOp(source);
            else
                return new SelectOp(source, selectOp.Variables);
        }

        private bool IsProjectionOnly(SelectOp selectOp)
        {
            if (selectOp.IsSelectAll)
                return true;
            else if (selectOp.Variables.Where(x => x.IsAggregate).Any())
                return false;
            else
                return true;
        }
    }
}
