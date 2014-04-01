using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    public class DistinctOp : ISparqlQueryModifier
    {
        public DistinctOp(ISparqlQuery innerQuery)
        {
            this.InnerQuery = innerQuery;
        }

        public ISparqlQuery InnerQuery { get; private set; }

        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield return InnerQuery;
        }

        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            if (originalQuery == InnerQuery)
                InnerQuery = newQuery;
        }

        public ISparqlQuery FinalizeAfterTransform()
        {
            if (InnerQuery is NoSolutionOp)
                return InnerQuery;
            else
                return this;
        }

        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
