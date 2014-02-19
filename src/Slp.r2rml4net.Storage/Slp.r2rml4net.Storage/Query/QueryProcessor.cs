using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Slp.r2rml4net.Storage.Query
{
    public class QueryProcessor
    {
        private MappingWrapper mapping;
        private ISqlDb db;

        public QueryProcessor(MappingWrapper mapping, ISqlDb db)
        {
            this.mapping = mapping;
            this.db = db;
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            var originalQuery = parser.ParseFromString(sparqlQuery);

            var context = new QueryContext(originalQuery, mapping);
            var sparqlProcessor = new SparqlProcessor(context);
            sparqlProcessor.Process();

            throw new NotImplementedException();
        }
    }
}
