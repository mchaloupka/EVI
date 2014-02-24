using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra
{
    [DebuggerDisplay("SELECT({InnerQuery})")]
    public class SelectOp : ISparqlQueryModifier
    {
        private List<VDS.RDF.Query.SparqlVariable> variables;

        public SelectOp(ISparqlQuery innerQuery)
        {
            this.InnerQuery = innerQuery;
            this.variables = null;
        }

        public SelectOp(ISparqlQuery innerQuery, IEnumerable<VDS.RDF.Query.SparqlVariable> variables)
        {
            this.InnerQuery = innerQuery;
            this.variables = variables.ToList();
        }


        public ISparqlQuery InnerQuery { get; private set; }

        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield return InnerQuery;
        }

        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            if (InnerQuery == originalQuery)
                InnerQuery = newQuery;
        }

        public override string ToString()
        {
            return string.Format("SELECT({0})", InnerQuery);
        }

        public ISparqlQuery FinalizeAfterTransform()
        {
            return this;
        }
    }
}
