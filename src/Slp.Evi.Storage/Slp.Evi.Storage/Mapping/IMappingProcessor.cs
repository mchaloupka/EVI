using Slp.Evi.Storage.Sparql.PostProcess;
using TCode.r2rml4net;

namespace Slp.Evi.Storage.Mapping
{
    /// <summary>
    /// Processor for R2RML mapping
    /// </summary>
    public interface IMappingProcessor
    {
        /// <summary>
        /// Gets the R2RML cache.
        /// </summary>
        /// <value>The R2RML cache.</value>
        R2RMLCache Cache { get; }

        /// <summary>
        /// Gets the R2RML mapping.
        /// </summary>
        /// <value>The R2RML mapping.</value>
        IR2RML Mapping { get; }

        /// <summary>
        /// Gets the mapping transformer.
        /// </summary>
        ISparqlPostProcess GetMappingTransformer();
    }
}