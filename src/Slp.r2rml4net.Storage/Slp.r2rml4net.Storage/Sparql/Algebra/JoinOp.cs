using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra
{
    [DebuggerDisplay("JOIN(Count = {joined.Count})")]
    public class JoinOp : ISparqlQueryPart
    {
        private List<ISparqlQuery> joined;

        public JoinOp()
        {
            joined = new List<ISparqlQuery>();
        }

        public void AddToJoin(Sparql.ISparqlQuery sparqlQuery)
        {
            joined.Add(sparqlQuery);
        }

        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            return joined.AsEnumerable();
        }

        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            var i = joined.IndexOf(originalQuery);

            if (i > -1)
            {
                if (newQuery is OneEmptySolutionOp)
                {
                    joined.RemoveAt(i);
                }
                else
                {
                    joined[i] = newQuery;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("JOIN({0})", string.Join(",", joined));
        }

        public ISparqlQuery FinalizeAfterTransform()
        {
            if (joined.OfType<NoSolutionOp>().Any())
            {
                return new NoSolutionOp();
            }
            else if (joined.Count == 0)
            {
                return new OneEmptySolutionOp();
            }
            else if (joined.Count == 1)
            {
                return joined[0];
            }
            else
            {
                return this;
            }
        }

        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
