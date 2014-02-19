using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slp.r2rml4net.Server.Models.Json
{
    public class JsonResultModel<T>
    {
        public bool Success { get; set; }

        public string ExceptionName { get; set; }

        public string ExceptionMessage { get; set; }

        public T Data { get; set; }

        public JsonResultModel()
        {
            Success = true;
            ExceptionName = null;
            ExceptionMessage = null;
            Data = default(T);
        }
    }
}