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

        public override string ToString()
        {
            return string.Format("UNION({0})", string.Join(",", unioned));
        }


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

        [DebuggerStepThrough]
        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
