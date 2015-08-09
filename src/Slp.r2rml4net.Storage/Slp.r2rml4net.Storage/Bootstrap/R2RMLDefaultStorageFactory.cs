using System.Collections.Generic;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Builder;
using Slp.r2rml4net.Storage.Relational.Optimization;
using Slp.r2rml4net.Storage.Relational.Optimization.Optimizers;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Bootstrap
{
    /// <summary>
    /// Class DefaultR2RMLStorageFactory.
    /// </summary>
    public class R2RMLDefaultStorageFactory
        : IR2RMLStorageFactory
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
        }
    }
}
