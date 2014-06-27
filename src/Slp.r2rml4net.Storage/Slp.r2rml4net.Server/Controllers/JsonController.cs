using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Slp.r2rml4net.Server.Models.Json;
using Slp.r2rml4net.Server.R2RML;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Server.Controllers
{
    public class JsonController : Controller
    {
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GetQueryResult(string query)
        {
            var result = new JsonResultModel<QueryResultModel>();
            result.Data = new QueryResultModel();

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var data = StorageWrapper.Storage.Query(query);
                sw.Stop();

                result.Data.TimeElapsedInMs = sw.ElapsedMilliseconds;

                if (data is Graph)
                {
                    var g = (Graph)data;

                    foreach (var triple in g.Triples)
                    {
                        var row = new QueryResultRowModel();
                        row.Bindings.Add(new QueryResultColumnModel()
                        {
                            Variable = "predicate",
                            Value = triple.Predicate.ToString()
                        });

                        row.Bindings.Add(new QueryResultColumnModel()
                        {
                            Variable = "subject",
                            Value = triple.Subject.ToString()
                        });

                        row.Bindings.Add(new QueryResultColumnModel()
                        {
                            Variable = "object",
                            Value = triple.Object.ToString()
                        });
                        result.Data.Rows.Add(row);
                    }

                    result.Data.Variables.Add("subject");
                    result.Data.Variables.Add("predicate");
                    result.Data.Variables.Add("object");
                }
                else if (data is SparqlResultSet)
                {
                    var resData = (SparqlResultSet)data;

                    foreach (var rowData in resData)
                    {
                        var row = new QueryResultRowModel();

                        foreach (var variable in rowData.Variables)
                        {
                            row.Bindings.Add(new QueryResultColumnModel()
                            {
                                Variable = variable,
                                Value = rowData[variable].ToString()
                            });
                        }

                        result.Data.Rows.Add(row);
                    }

                    result.Data.Variables.AddRange(resData.Variables);
                }
                else
                {
                    throw new Exception("Unknown query result");
                }

                result.Success = true;
                return Json(result);
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Data = null;
                result.ExceptionMessage = e.Message;
                result.ExceptionName = e.GetType().Name;
                return Json(result);
            }
        }

        private UrlHelper GetUrlHelper()
        {
            return new UrlHelper(HttpContext.Request.RequestContext);
        }
    }
}