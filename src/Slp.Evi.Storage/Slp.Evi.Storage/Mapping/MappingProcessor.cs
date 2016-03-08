using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using TCode.r2rml4net;

namespace Slp.Evi.Storage.Mapping
{
    /// <summary>
    /// Processor for R2RML mapping
    /// </summary>
    public class MappingProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProcessor"/> class.
        /// </summary>
        /// <param name="mapping">The R2RML mapping.</param>
        public MappingProcessor(IR2RML mapping)
        {
            Mapping = mapping;
            Cache = new R2RMLCache();
        }

        /// <summary>
        /// Gets the R2RML mapping.
        /// </summary>
        /// <value>The R2RML mapping.</value>
        public IR2RML Mapping { get; private set; }

        /// <summary>
        /// Gets the R2RML cache.
        /// </summary>
        /// <value>The R2RML cache.</value>
        public R2RMLCache Cache { get; private set; }

        /// <summary>
        /// Processes the SPARQL algebra.
        /// </summary>
        /// <param name="graphPattern">The SPARQL algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed SPARQL algebra.</returns>
        public IGraphPattern ProcessPattern(IGraphPattern graphPattern, QueryContext context)
        {
            var transformer = new MappingTransformer(this);
            return transformer.TransformGraphPattern(graphPattern, context);
        }
    }
}
