using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slp.r2rml4net.Server.Models.Json
{
    /// <summary>
    /// Query result model
    /// </summary>
    public class QueryResultModel
    {
        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public List<string> Variables { get; set; }

        /// <summary>
        /// Gets or sets the time elapsed in ms.
        /// </summary>
        /// <value>The time elapsed in ms.</value>
        public long TimeElapsedInMs { get; set; }

        /// <summary>
        /// Gets or sets the rows.
        /// </summary>
        /// <value>The rows.</value>
        public List<QueryResultRowModel> Rows { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResultModel"/> class.
        /// </summary>
        public QueryResultModel()
        {
            this.Rows = new List<QueryResultRowModel>();
            this.Variables = new List<string>();
        }
    }

    /// <summary>
    /// Row in query result model
    /// </summary>
    public class QueryResultRowModel
    {
        /// <summary>
        /// Gets or sets the bindings.
        /// </summary>
        /// <value>The bindings.</value>
        public List<QueryResultColumnModel> Bindings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResultRowModel"/> class.
        /// </summary>
        public QueryResultRowModel()
        {
            this.Bindings = new List<QueryResultColumnModel>();
        }
    }

    /// <summary>
    /// Column in query row
    /// </summary>
    public class QueryResultColumnModel
    {
        /// <summary>
        /// Gets or sets the variable.
        /// </summary>
        /// <value>The variable.</value>
        public string Variable { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }
    }
}