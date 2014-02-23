using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra
{
    [DebuggerDisplay("UNION(Count = {unioned.Count})")]
    public class UnionOp : ISparqlQueryPart
    {
        private List<ISparqlQuery> unioned;

        public UnionOp()
        {
            unioned = new List<ISparqlQuery>();
        }

        public void AddToUnion(Sparql.ISparqlQuery sparqlQuery)
        {
            unioned.Add(sparqlQuery);
        }

        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            return unioned.AsEnumerable();
        }


        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            var i = unioned.IndexOf(originalQuery);

            if (i > -1)
            {
                unioned[i] = newQuery;
            }
        }

        public override string ToString()
        {
            return string.Format("UNION({0})", string.Join(",", unioned));
        }
    }
}
