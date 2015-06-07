using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Database;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Bootstrap
{
    /// <summary>
    /// Factory for R2RML storage
    /// </summary>
    public interface IR2RmlStorageFactory
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
    }
}
