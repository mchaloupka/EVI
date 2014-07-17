using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;

namespace Slp.r2rml4net.Storage.Optimization
{
    /// <summary>
    /// Interface for SQL algebra optimizer
    /// </summary>
    public interface ISqlAlgebraOptimizer
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context);
    }

    /// <summary>
    /// Interface for SQL algebra optimizer during the algebra creation
    /// </summary>
    public interface ISqlAlgebraOptimizerOnTheFly
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        INotSqlOriginalDbSource ProcessAlgebraOnTheFly(INotSqlOriginalDbSource algebra, QueryContext context);
    }
}
