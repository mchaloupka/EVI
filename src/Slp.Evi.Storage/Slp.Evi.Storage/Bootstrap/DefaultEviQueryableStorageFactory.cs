using System.Collections.Generic;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Mapping;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Builder;
using Slp.Evi.Storage.Relational.Optimization;
using Slp.Evi.Storage.Relational.Optimization.Optimizers;
using Slp.Evi.Storage.Sparql.Builder;
using Slp.Evi.Storage.Sparql.Optimization;
using Slp.Evi.Storage.Sparql.Optimization.Optimizers;
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
        public virtual MappingProcessor CreateMappingProcessor(IR2RML mapping)
        {
            return new MappingProcessor(mapping);
        }

        /// <summary>
        /// Creates the sparql algebra builder.
        /// </summary>
        public virtual SparqlBuilder CreateSparqlBuilder()
        {
            return new SparqlBuilder();
        }

        /// <summary>
        /// Creates the query context.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="schemaProvider"></param>
        /// <param name="nodeFactory">The node factory.</param>
        public virtual QueryContext CreateQueryContext(SparqlQuery originalQuery, MappingProcessor mapping, ISqlDatabase db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory)
        {
            return new QueryContext(originalQuery, mapping, db, schemaProvider, nodeFactory, this);
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
        public virtual IEnumerable<IRelationalOptimizer> GetRelationalOptimizers()
        {
            yield return new ConcatenationInEqualConditionOptimizer();
            yield return new ConstantExpressionEqualityOptimizer();
            yield return new IsNullOptimizer();
            yield return new SelfJoinOptimizer();
        }

        /// <summary>
        /// Gets the SPARQL optimizers.
        /// </summary>
        public IEnumerable<ISparqlOptimizer> GetSparqlOptimizers()
        {
            yield return new TriplePatternOptimizer();
            yield return new UnionJoinOptimizer();
        }
    }
}
