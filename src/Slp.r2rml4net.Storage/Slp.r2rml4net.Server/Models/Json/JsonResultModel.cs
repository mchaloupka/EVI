using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slp.r2rml4net.Server.Models.Json
{
    /// <summary>
    /// Result model for json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonResultModel<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="JsonResultModel{T}"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the name of the exception.
        /// </summary>
        /// <value>The name of the exception.</value>
        public string ExceptionName { get; set; }

        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        /// <value>The exception message.</value>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public T Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResultModel{T}"/> class.
        /// </summary>
        public JsonResultModel()
        {
            Success = true;
            ExceptionName = null;
            ExceptionMessage = null;
            Data = default(T);
        }
    }
}