using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using Slp.r2rml4net.Storage.Sparql.Utils;

namespace Slp.r2rml4net.Storage.Mapping
{
    /// <summary>
    /// Mapping transformer
    /// </summary>
    public class MappingTransformer
        : BaseSparqlTransformer<QueryContext>
    {
        /// <summary>
        /// The mapping processor
        /// </summary>
        private MappingProcessor _mappingProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingTransformer"/> class.
        /// </summary>
        /// <param name="mappingProcessor">The mapping processor.</param>
        public MappingTransformer(MappingProcessor mappingProcessor)
        {
            _mappingProcessor = mappingProcessor;
        }

        /// <summary>
        /// Processes the specified triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected override IGraphPattern Process(TriplePattern triplePattern, QueryContext data)
        {
            return triplePattern;
        }
    }
}