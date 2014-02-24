using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra
{
    [DebuggerDisplay("None")]
    public class NoSolutionOp : ISparqlQueryPart
    {
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield break;
        }

        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            throw new Exception("Should not be called, NoSolutionOp has no subqueries");
        }

        public override string ToString()
        {
            return "None";
        }

        public ISparqlQuery FinalizeAfterTransform()
        {
            return this;
        }
    }
}
