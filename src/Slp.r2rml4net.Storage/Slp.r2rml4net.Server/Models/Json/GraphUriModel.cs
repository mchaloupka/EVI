using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Slp.r2rml4net.Server.Models.Json
{
    public class GraphUriModel
    {
        public GraphUriModel(Uri uri, UrlHelper urlHelper)
        {
            this.Uri = uri;
            this.Link = urlHelper.Action("Graph", "Main", new { uri = uri });
        }

        public Uri Uri { get; set; }

        public string Link { get; set; }
    }
}