using System.Collections.Generic;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Mapping;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Builder;
using Slp.Evi.Storage.Relational.PostProcess;
using Slp.Evi.Storage.Sparql.Builder;
using Slp.Evi.Storage.Sparql.PostProcess;
using Slp.Evi.Storage.Sparql.Types;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.Evi.Storage.Bootstrap
{
    /// <summary>
    /// Factory for <see cref="EviQueryableStorage"/>
    /// </summary>
    public interface IEviQueryableStorageFactory
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
        IMappingProcessor CreateMappingProcessor(IR2RML mapping);

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
        /// <param name="typeCache">The type cache.</param>
        IQueryContext CreateQueryContext(SparqlQuery originalQuery, IMappingProcessor mapping, ISqlDatabase db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory, ITypeCache typeCache);

        /// <summary>
        /// Creates the relational builder.
        /// </summary>
        /// <returns>RelationalBuilder.</returns>
        RelationalBuilder CreateRelationalBuilder();

        /// <summary>
        /// Gets the relational post processes.
        /// </summary>
        IEnumerable<IRelationalPostProcess> GetRelationalPostProcesses();

        /// <summary>
        /// Gets the SPARQL post processes.
        /// </summary>
        /// <param name="mapping">Used mapping processor</param>
        IEnumerable<ISparqlPostProcess> GetSparqlPostProcesses(IMappingProcessor mapping);
    }
}
