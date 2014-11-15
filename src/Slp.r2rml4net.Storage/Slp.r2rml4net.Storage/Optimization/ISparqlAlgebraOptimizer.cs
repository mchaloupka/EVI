using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;

namespace Slp.r2rml4net.Storage.Optimization
{
    /// <summary>
    /// Interface for SPARQL algebra optimizer
    /// </summary>
    public interface ISparqlAlgebraOptimizer
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context);
    }

    /// <summary>
    /// Interface for SPARQL algebra optimizer on the fly
    /// </summary>
    public interface ISparqlAlgebraOptimizerOnTheFly
    {
        /// <summary>
        /// Processes the algebra on the fly.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        ISparqlQuery ProcessAlgebraOnTheFly(ISparqlQuery algebra, QueryContext context);
    }
}
