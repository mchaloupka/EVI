using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sparql
{
    public class SparqlProcessor
    {
        private QueryContext context;

        public SparqlProcessor(QueryContext context)
        {
            this.context = context;
        }

        public void Process()
        {
            var originalQuery = this.context.OriginalQuery;

            switch (originalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    ProcessAsk(originalQuery);
                    break;
                case SparqlQueryType.Construct:
                    ProcessConstruct(originalQuery);
                    break;
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    ProcessDescribe(originalQuery);
                    break;
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    ProcessSelect(originalQuery);
                    break;
                default:
                    throw new Exception("Cannot handle unknown query type");
            }
        }

        private void ProcessAsk(SparqlQuery originalQuery)
        {
            throw new NotImplementedException();
        }

        private void ProcessConstruct(SparqlQuery originalQuery)
        {
            throw new NotImplementedException();
        }

        private void ProcessDescribe(SparqlQuery originalQuery)
        {
            throw new NotImplementedException();
        }

        private void ProcessSelect(SparqlQuery originalQuery)
        {
            //var originalAlgebra = originalQuery.ToAlgebra();
            throw new NotImplementedException();
        }
    }
}
