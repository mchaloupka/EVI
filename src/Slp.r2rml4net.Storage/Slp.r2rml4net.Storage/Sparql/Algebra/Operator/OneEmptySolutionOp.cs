using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    [DebuggerDisplay("Empty")]
    public class OneEmptySolutionOp : ISparqlQueryPart
    {
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield break;
        }

        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            throw new Exception("Should not be called, OneEmptySolutionOp has no subqueries");
        }

        public override string ToString()
        {
            return "Empty";
        }

        public ISparqlQuery FinalizeAfterTransform()
        {
            return this;
        }

        [DebuggerStepThrough]
        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
