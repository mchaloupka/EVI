using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    [DebuggerDisplay("SELECT({InnerQuery})")]
    public class SelectOp : ISparqlQueryModifier
    {
        private List<SparqlVariable> variables;

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

        public bool IsSelectAll { get { return variables == null; } }

        public IEnumerable<SparqlVariable> Variables { get { return variables; } }

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

        [DebuggerStepThrough]
        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
