using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slp.r2rml4net.Server.Models.Json
{
    public class QueryResultModel
    {
        public List<string> Variables { get; set; }

        public long TimeElapsedInMs { get; set; }

        public List<QueryResultRowModel> Rows { get; set; }

        public QueryResultModel()
        {
            this.Rows = new List<QueryResultRowModel>();
            this.Variables = new List<string>();
        }
    }

    public class QueryResultRowModel
    {
        public List<QueryResultColumnModel> Bindings { get; set; }

        public QueryResultRowModel()
        {
            this.Bindings = new List<QueryResultColumnModel>();
        }
    }

    public class QueryResultColumnModel
    {
        public string Variable { get; set; }

        public string Value { get; set; }
    }
}