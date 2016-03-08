using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;

namespace Slp.r2rml4net.Storage.Relational.Optimization
{
    /// <summary>
    /// Interface for relational optimization
    /// </summary>
    public interface IRelationalOptimizer
    {
        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        RelationalQuery Optimize(RelationalQuery query, QueryContext context);
    }
}
