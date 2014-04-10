using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    public class OrderByOp : ISparqlQueryModifier
    {
        private List<OrderByComparator> orderings;

        public OrderByOp(ISparqlQuery innerQuery)
        {
            this.InnerQuery = innerQuery;
            this.orderings = new List<OrderByComparator>();
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

        [DebuggerStepThrough]
        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void AddOrdering(ISparqlQueryExpression sparqlQueryExpression, bool descending)
        {
            this.orderings.Add(new OrderByComparator(sparqlQueryExpression, descending));
        }

        public IEnumerable<OrderByComparator> Orderings { get { return orderings; } }
    }

    public class OrderByComparator
    {
        public ISparqlQueryExpression Expression { get; private set; }
        public bool Descending { get; private set; }

        public OrderByComparator(ISparqlQueryExpression expression, bool descending)
        {
            this.Expression = expression;
            this.Descending = descending;
        }

    }
}
