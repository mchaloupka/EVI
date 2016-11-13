using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Relational.PostProcess;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.PostProcess;

namespace Slp.Evi.Storage.Query
{
    /// <summary>
    /// Class QueryPostProcess.
    /// </summary>
    public class QueryPostProcesses
    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly IQueryContext _context;

        /// <summary>
        /// The relational optimizers
        /// </summary>
        private readonly List<IRelationalPostProcess> _relationalPostprocesses;

        /// <summary>
        /// The SPARQL optimizers
        /// </summary>
        private readonly List<ISparqlPostProcess> _sparqlPostprocesses;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryPostProcesses" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        public QueryPostProcesses(IEviQueryableStorageFactory factory, IQueryContext context)
        {
            _context = context;
            _relationalPostprocesses = new List<IRelationalPostProcess>(factory.GetRelationalPostProcesses());
            _sparqlPostprocesses = new List<ISparqlPostProcess>(factory.GetSparqlPostProcesses(context.Mapping));
        }

        /// <summary>
        /// Optimizes the algebra on the fly.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <returns>The optimized algebra.</returns>
        public RelationalQuery PostProcess(RelationalQuery algebra)
        {
            return _relationalPostprocesses.Aggregate(algebra, (current, processor) => processor.Process(current, _context));
        }

        /// <summary>
        /// Optimizes the algebra.
        /// </summary>
        /// <param name="algebra">The SPARQL query.</param>
        /// <returns>The optimized SPARQL query.</returns>
        public ISparqlQuery PostProcess(ISparqlQuery algebra)
        {
            return _sparqlPostprocesses.Aggregate(algebra, (current, processor) => processor.Process(current, _context));
        }
    }
}