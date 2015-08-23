using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Relational.Optimization;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Optimization;

namespace Slp.r2rml4net.Storage.Query
{
    /// <summary>
    /// Class Optimizers.
    /// </summary>
    public class Optimizers
    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly QueryContext _context;

        /// <summary>
        /// The relational optimizers
        /// </summary>
        private List<IRelationalOptimizer> _relationalOptimizers;

        /// <summary>
        /// The SPARQL optimizers
        /// </summary>
        private List<ISparqlOptimizer> _sparqlOptimizers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Optimizers" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        public Optimizers(IR2RMLStorageFactory factory, QueryContext context)
        {
            _context = context;
            this._relationalOptimizers = new List<IRelationalOptimizer>(factory.GetRelationalOptimizers());
            this._sparqlOptimizers = new List<ISparqlOptimizer>(factory.GetSparqlOptimizers());
        }

        /// <summary>
        /// Optimizes the algebra on the fly.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <returns>The optimized algebra.</returns>
        public RelationalQuery Optimize(RelationalQuery algebra)
        {
            return _relationalOptimizers.Aggregate(algebra, (current, optimizer) => optimizer.Optimize(current, _context));
        }

        /// <summary>
        /// Optimizes the algebra on the fly.
        /// </summary>
        /// <param name="algebra">The SPARQL query.</param>
        /// <returns>The optimized SPARQL query.</returns>
        public ISparqlQuery Optimize(ISparqlQuery algebra)
        {
            return _sparqlOptimizers.Aggregate(algebra, (current, optimizer) => optimizer.Optimize(current, _context));
        }
    }
}