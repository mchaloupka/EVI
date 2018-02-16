using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Mapping;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Builder;
using Slp.Evi.Storage.Relational.PostProcess;
using Slp.Evi.Storage.Relational.PostProcess.Optimizers;
using Slp.Evi.Storage.Sparql.Builder;
using Slp.Evi.Storage.Sparql.PostProcess;
using Slp.Evi.Storage.Sparql.PostProcess.Optimizers;
using Slp.Evi.Storage.Sparql.PostProcess.SafeAlgebra;
using Slp.Evi.Storage.Types;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.Evi.Storage.Bootstrap
{
    /// <summary>
    /// Class DefaultEviQueryableStorageFactory.
    /// </summary>
    public class DefaultEviQueryableStorageFactory
        : IEviQueryableStorageFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEviQueryableStorageFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public DefaultEviQueryableStorageFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Creates the query processor.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="mapping">The mapping.</param>
        public virtual QueryProcessor CreateQueryProcessor(ISqlDatabase db, IR2RML mapping)
        {
            return new QueryProcessor(db, mapping, this);
        }

        /// <summary>
        /// Creates the mapping processor.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        public virtual IMappingProcessor CreateMappingProcessor(IR2RML mapping)
        {
            return new MappingProcessor(mapping);
        }

        /// <summary>
        /// Creates the sparql algebra builder.
        /// </summary>
        public virtual SparqlBuilder CreateSparqlBuilder()
        {
            return new SparqlBuilder(_loggerFactory.CreateLogger<SparqlBuilder>());
        }

        /// <summary>
        /// Creates the query context.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="schemaProvider"></param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="typeCache">The type cache.</param>
        public virtual IQueryContext CreateQueryContext(SparqlQuery originalQuery, IMappingProcessor mapping, ISqlDatabase db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory, ITypeCache typeCache)
        {
            return new QueryContext(originalQuery, mapping, db, schemaProvider, nodeFactory, this, typeCache);
        }

        /// <summary>
        /// Creates the relational builder.
        /// </summary>
        /// <returns>The relational builder.</returns>
        public virtual RelationalBuilder CreateRelationalBuilder()
        {
            return new RelationalBuilder();
        }

        /// <summary>
        /// Gets the relational optimizers.
        /// </summary>
        public virtual IEnumerable<IRelationalPostProcess> GetRelationalPostProcesses()
        {
            yield return new CaseExpressionToConditionOptimizer();
            yield return new ConcatenationInEqualConditionOptimizer();
            yield return new ConstantExpressionEqualityOptimizer();
            yield return new IsNullOptimizer();
            yield return new SelfJoinOptimizer();
        }

        /// <summary>
        /// Gets the SPARQL optimizers.
        /// </summary>
        /// <param name="mapping">Used mapping processor</param>
        public IEnumerable<ISparqlPostProcess> GetSparqlPostProcesses(IMappingProcessor mapping)
        {
            yield return new AscendFilterPattern();
            yield return mapping.GetMappingTransformer();
            yield return new TriplePatternOptimizer();
            yield return new UnionJoinOptimizer();
        }
    }
}
