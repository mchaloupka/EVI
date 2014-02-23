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
        private MappingProcessor mapping;
        private ISqlDb db;

        public QueryProcessor(MappingProcessor mapping, ISqlDb db)
        {
            this.mapping = mapping;
            this.db = db;
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            var originalQuery = parser.ParseFromString(sparqlQuery);

            // Convert to algebra
            var context = new QueryContext(originalQuery, mapping);
            var sparqlAlgebraBuilder = new SparqlAlgebraBuilder(context);
            var algebra = sparqlAlgebraBuilder.Process();

            // Transform graph and from statements

            // Transform using R2RML
            algebra = mapping.ProcessAlgebra(algebra);

            // TODO: Validate algebra, take filters and union up as possible

            // TODO: Optimize

            // TODO: Transform to SQL

            // TODO: Query

            // TODO: Process results

            throw new NotImplementedException();
        }
    }
}
