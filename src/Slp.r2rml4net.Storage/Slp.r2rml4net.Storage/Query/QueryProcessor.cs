using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Optimization;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sql;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Slp.r2rml4net.Storage.Query
{
    public class QueryProcessor
    {
        private MappingProcessor mapping;
        private ISqlDb db;
        private SparqlAlgebraBuilder sparqlAlgebraBuilder;
        private List<ISparqlAlgebraOptimizer> sparqlOptimizers;
        private SqlAlgebraBuilder sqlAlgebraBuilder;

        public QueryProcessor(MappingProcessor mapping, ISqlDb db)
        {
            this.mapping = mapping;
            this.db = db;
            this.sparqlAlgebraBuilder = new SparqlAlgebraBuilder();
            this.sqlAlgebraBuilder = new SqlAlgebraBuilder();

            this.sparqlOptimizers = new List<ISparqlAlgebraOptimizer>()
            {
                new Optimization.SparqlAlgebra.R2RMLOptimizer()
            };
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            var originalQuery = parser.ParseFromString(sparqlQuery);

            // Convert to algebra
            var context = new QueryContext(originalQuery, mapping);

            // Generate SQL algebra
            var sqlAlgebra = GenerateSqlAlgebra(context);

            // TODO: Query

            // TODO: Process results

            throw new NotImplementedException();
        }

        private ISqlQuery GenerateSqlAlgebra(QueryContext context)
        {
            var algebra = sparqlAlgebraBuilder.Process(context);

            // Transform graph and from statements

            // Transform using R2RML
            algebra = mapping.ProcessAlgebra(algebra, context);

            // TODO: Validate algebra, take filters and union up as possible

            // Optimize sparql algebra
            foreach (var optimizer in sparqlOptimizers)
            {
                algebra = optimizer.ProcessAlgebra(algebra, context);
            }

            // Transform to SQL algebra
            var sqlAlgebra = sqlAlgebraBuilder.Process(algebra, context);
            
            // TODO: Optimize sql algebra

            return sqlAlgebra;
        }
    }
}
