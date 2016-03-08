using System.Collections.Generic;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Mapping;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Builder;
using Slp.Evi.Storage.Relational.Optimization;
using Slp.Evi.Storage.Sparql.Builder;
using Slp.Evi.Storage.Sparql.Optimization;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.Evi.Storage.Bootstrap
{
    /// <summary>
    /// Factory for R2RML storage
    /// </summary>
    public interface IR2RMLStorageFactory
    {
        /// <summary>
        /// Creates the query processor.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="mapping">The R2RML mapping.</param>
        QueryProcessor CreateQueryProcessor(ISqlDatabase db, IR2RML mapping);

        /// <summary>
        /// Creates the mapping processor.
        /// </summary>
        /// <param name="mapping">The R2MRML mapping.</param>
        MappingProcessor CreateMappingProcessor(IR2RML mapping);

        /// <summary>
        /// Creates the sparql algebra builder.
        /// </summary>
        SparqlBuilder CreateSparqlBuilder();

        /// <summary>
        /// Creates the query context.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="schemaProvider"></param>
        /// <param name="nodeFactory">The node factory.</param>
        QueryContext CreateQueryContext(SparqlQuery originalQuery, MappingProcessor mapping, ISqlDatabase db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory);

        /// <summary>
        /// Creates the relational builder.
        /// </summary>
        /// <returns>RelationalBuilder.</returns>
        RelationalBuilder CreateRelationalBuilder();

        /// <summary>
        /// Gets the relational optimizers.
        /// </summary>
        IEnumerable<IRelationalOptimizer> GetRelationalOptimizers();

        /// <summary>
        /// Gets the SPARQL optimizers.
        /// </summary>
        IEnumerable<ISparqlOptimizer> GetSparqlOptimizers();
    }
}
