using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Sql;
using VDS.RDF;

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
            throw new NotImplementedException();
        }
    }
}
