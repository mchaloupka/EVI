using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query.Source;

namespace Slp.r2rml4net.Storage.Relational.Database
{
    /// <summary>
    /// The SQL query builder
    /// </summary>
    public interface ISqlQueryBuilder
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="calculusModel">The calculus model.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.String.</returns>
        string GenerateQuery(CalculusModel calculusModel, QueryContext context);
    }
}
