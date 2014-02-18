using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slp.r2rml4net.Server.Models.Main
{
    public class GraphModel
    {
        public GraphModel(Uri uri)
        {
            this.GraphUri = uri;
        }
        public Uri GraphUri { get; set; }
    }
}