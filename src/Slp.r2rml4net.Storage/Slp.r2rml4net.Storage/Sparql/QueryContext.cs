using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql
{
    public class QueryContext
    {
        public QueryContext(SparqlQuery query, MappingProcessor mapping)
        {
            this.OriginalQuery = query;
            this.Mapping = mapping;
        }

        public SparqlQuery OriginalQuery { get; private set; }

        public MappingProcessor Mapping { get; private set; }
    }
}
