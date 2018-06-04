using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Mapping.Representation.Implementation;
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
            TriplesMaps = CreateMappingRepresentation(mapping).ToArray();
        }

        private IEnumerable<ITriplesMapping> CreateMappingRepresentation(IR2RML mapping)
        {
            var creationContext = new RepresentationCreationContext();
            return mapping.TriplesMaps.Select(x => TriplesMapping.Create(x, creationContext));
        }

        /// <summary>
        /// Gets the mapping.
        /// </summary>
        /// <value>Collection of triple maps.</value>
        public IEnumerable<ITriplesMapping> TriplesMaps { get; }

        /// <summary>
        /// Gets the mapping transformer.
        /// </summary>
        public ISparqlPostProcess GetMappingTransformer()
        {
            return new MappingTransformer(this, _loggerFactory.CreateLogger<MappingTransformer>());
        }
    }
}
