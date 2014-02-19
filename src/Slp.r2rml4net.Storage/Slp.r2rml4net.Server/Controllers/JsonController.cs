using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Slp.r2rml4net.Server.Models.Json;
using Slp.r2rml4net.Server.R2RML;

namespace Slp.r2rml4net.Server.Controllers
{
    public class JsonController : Controller
    {
        [HttpPost]
        public ActionResult GetGraphList()
        {
            var result = new JsonResultModel<List<GraphUriModel>>();

            try
            {
                var urlHelper = GetUrlHelper();
                var data = StorageWrapper.Storage.ListGraphs().ToList();
                result.Data = data.Select(x => new GraphUriModel(x, urlHelper)).ToList();
                result.Success = true;
                return Json(result);


            }
            catch (Exception e)
            {
                result.Success = false;
                result.ExceptionMessage = e.Message;
                result.ExceptionName = e.GetType().Name;
                result.Data = null;
                return Json(result);
            }
        }

        private UrlHelper GetUrlHelper()
        {
            return new UrlHelper(HttpContext.Request.RequestContext);
        }
    }
}