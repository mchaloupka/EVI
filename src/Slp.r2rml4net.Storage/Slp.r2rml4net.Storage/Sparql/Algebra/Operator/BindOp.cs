using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    public class BindOp : ISparqlQueryPart
    {
        public BindOp(string varName, ISparqlQueryExpression expression, ISparqlQuery innerQuery)
        {
            this.VariableName = varName;
            this.Expression = expression;
            this.InnerQuery = innerQuery;
        }

        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield return InnerQuery;
        }

        public void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery)
        {
            if (InnerQuery == originalQuery)
                InnerQuery = newQuery;
        }

        public ISparqlQuery FinalizeAfterTransform()
        {
            return this;
        }

        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public string VariableName { get; set; }

        public ISparqlQueryExpression Expression { get; set; }

        public ISparqlQuery InnerQuery { get; private set; }
    }
}
