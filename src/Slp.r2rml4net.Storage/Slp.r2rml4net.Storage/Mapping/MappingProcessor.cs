using System;
using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Utils;
using TCode.r2rml4net;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Mapping
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
            Cache = new R2RmlCache();
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
        public R2RmlCache Cache { get; private set; }

        /// <summary>
        /// Processes the SPARQL algebra.
        /// </summary>
        /// <param name="algebra">The SPARQL algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed SPARQL algebra.</returns>
        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            var transformer = new MappingTransformer(this);

            return transformer.TransformSparqlQuery(algebra, context);
        }
    }
}
