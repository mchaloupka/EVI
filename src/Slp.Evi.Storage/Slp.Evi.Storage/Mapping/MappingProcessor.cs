using Microsoft.Extensions.Logging;
using Slp.Evi.Storage.Sparql.PostProcess;
using TCode.r2rml4net;

namespace Slp.Evi.Storage.Mapping
{
    /// <summary>
    /// Processor for R2RML mapping
    /// </summary>
    public class MappingProcessor : IMappingProcessor
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProcessor"/> class.
        /// </summary>
        /// <param name="mapping">The R2RML mapping.</param>
        /// <param name="loggerFactory">The logger factory</param>
        public MappingProcessor(IR2RML mapping, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Mapping = mapping;
            Cache = new R2RMLCache();
        }

        /// <summary>
        /// Gets the R2RML mapping.
        /// </summary>
        /// <value>The R2RML mapping.</value>
        public IR2RML Mapping { get; }

        /// <summary>
        /// Gets the R2RML cache.
        /// </summary>
        /// <value>The R2RML cache.</value>
        public R2RMLCache Cache { get; }

        /// <summary>
        /// Gets the mapping transformer.
        /// </summary>
        public ISparqlPostProcess GetMappingTransformer()
        {
            return new MappingTransformer(this, _loggerFactory.CreateLogger<MappingTransformer>());
        }
    }
}
