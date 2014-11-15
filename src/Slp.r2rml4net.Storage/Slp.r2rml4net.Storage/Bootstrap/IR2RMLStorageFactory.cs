using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Bootstrap
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
        Query.QueryProcessor CreateQueryProcessor(Sql.ISqlDb db, TCode.r2rml4net.IR2RML mapping);

        /// <summary>
        /// Creates the mapping processor.
        /// </summary>
        /// <param name="mapping">The R2MRML mapping.</param>
        Mapping.MappingProcessor CreateMappingProcessor(TCode.r2rml4net.IR2RML mapping);

        /// <summary>
        /// Creates the sparql algebra builder.
        /// </summary>
        Sparql.SparqlAlgebraBuilder CreateSparqlAlgebraBuilder();

        /// <summary>
        /// Creates the SQL algebra builder.
        /// </summary>
        Sql.SqlAlgebraBuilder CreateSqlAlgebraBuilder();

        /// <summary>
        /// Creates the sparql algebra optimizers.
        /// </summary>
        Optimization.ISparqlAlgebraOptimizer[] CreateSparqlAlgebraOptimizers();

        /// <summary>
        /// Creates the sparql algebra optimizers on the fly.
        /// </summary>
        Optimization.ISparqlAlgebraOptimizerOnTheFly[] CreateSparqlAlgebraOptimizersOnTheFly();

        /// <summary>
        /// Creates the SQL optimizers.
        /// </summary>
        Optimization.ISqlAlgebraOptimizer[] CreateSqlOptimizers();

        /// <summary>
        /// Creates the SQL algebra optimizers on the fly.
        /// </summary>
        Optimization.ISqlAlgebraOptimizerOnTheFly[] CreateSqlAlgebraOptimizersOnTheFly();

        /// <summary>
        /// Creates the query context.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="sparqlAlgebraOptimizerOnTheFly">The sparql algebra optimizer on the fly.</param>
        /// <param name="sqlAlgebraOptimizerOnTheFly">The SQL algebra optimizers on the fly.</param>
        Slp.r2rml4net.Storage.Query.QueryContext CreateQueryContext(VDS.RDF.Query.SparqlQuery originalQuery, Mapping.MappingProcessor mapping, Sql.ISqlDb db, VDS.RDF.INodeFactory nodeFactory, Optimization.ISparqlAlgebraOptimizerOnTheFly[] sparqlAlgebraOptimizerOnTheFly, Optimization.ISqlAlgebraOptimizerOnTheFly[] sqlAlgebraOptimizerOnTheFly);
    }
}
