using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Optimization;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Old;
using Slp.r2rml4net.Storage.Sql;
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
        QueryProcessor CreateQueryProcessor(ISqlDb db, IR2RML mapping);

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
        /// Creates the sparql algebra builder.
        /// </summary>
        SparqlAlgebraBuilder CreateSparqlAlgebraBuilder();

        /// <summary>
        /// Creates the SQL algebra builder.
        /// </summary>
        SqlAlgebraBuilder CreateSqlAlgebraBuilder();

        /// <summary>
        /// Creates the sparql algebra optimizers.
        /// </summary>
        ISparqlAlgebraOptimizer[] CreateSparqlAlgebraOptimizers();

        /// <summary>
        /// Creates the sparql algebra optimizers on the fly.
        /// </summary>
        ISparqlAlgebraOptimizerOnTheFly[] CreateSparqlAlgebraOptimizersOnTheFly();

        /// <summary>
        /// Creates the SQL optimizers.
        /// </summary>
        ISqlAlgebraOptimizer[] CreateSqlOptimizers();

        /// <summary>
        /// Creates the SQL algebra optimizers on the fly.
        /// </summary>
        ISqlAlgebraOptimizerOnTheFly[] CreateSqlAlgebraOptimizersOnTheFly();

        /// <summary>
        /// Creates the query context.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="schemaProvider"></param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="sparqlAlgebraOptimizerOnTheFly">The sparql algebra optimizer on the fly.</param>
        /// <param name="sqlAlgebraOptimizerOnTheFly">The SQL algebra optimizers on the fly.</param>
        QueryContext CreateQueryContext(SparqlQuery originalQuery, MappingProcessor mapping, ISqlDb db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory, ISparqlAlgebraOptimizerOnTheFly[] sparqlAlgebraOptimizerOnTheFly, ISqlAlgebraOptimizerOnTheFly[] sqlAlgebraOptimizerOnTheFly);
    }
}
