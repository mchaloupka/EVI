using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slp.r2rml4net.Server.R2RML
{
    public class QueryProcessorWrapper : VDS.RDF.Query.GenericQueryProcessor
    {
        public QueryProcessorWrapper()
            : base(StorageWrapper.Storage)
        {

        }
    }
}