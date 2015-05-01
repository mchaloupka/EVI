using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Optimization;
using Slp.r2rml4net.Storage.Optimization.SparqlAlgebra;
using Slp.r2rml4net.Storage.Optimization.SqlAlgebra;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Bootstrap
{
    /// <summary>
    /// Class DefaultR2RMLStorageFactory.
    /// </summary>
    public class DefaultIr2RmlStorageFactory : IR2RmlStorageFactory
    {
        /// <summary>
        /// Creates the query processor.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="mapping">The mapping.</param>
        public virtual QueryProcessor CreateQueryProcessor(ISqlDb db, IR2RML mapping)
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
        public virtual SparqlAlgebraBuilder CreateSparqlAlgebraBuilder()
        {
            return new SparqlAlgebraBuilder();
        }

        /// <summary>
        /// Creates the SQL algebra builder.
        /// </summary>
        public virtual SqlAlgebraBuilder CreateSqlAlgebraBuilder()
        {
            return new SqlAlgebraBuilder();
        }

        /// <summary>
        /// Creates the sparql algebra optimizers.
        /// </summary>
        public virtual ISparqlAlgebraOptimizer[] CreateSparqlAlgebraOptimizers()
        {
            return new ISparqlAlgebraOptimizer[]
            {
                new R2RmlOptimizer(),
                new UnionOptimizer(),
                new JoinOptimizer(),
                new SelectIntoUnionOptimizer()
            };
        }

        /// <summary>
        /// Creates the sparql algebra optimizers on the fly.
        /// </summary>
        public virtual ISparqlAlgebraOptimizerOnTheFly[] CreateSparqlAlgebraOptimizersOnTheFly()
        {
            return new ISparqlAlgebraOptimizerOnTheFly[]
            {

            };
        }

        /// <summary>
        /// Creates the SQL optimizers.
        /// </summary>
        public virtual ISqlAlgebraOptimizer[] CreateSqlOptimizers()
        {
            return new ISqlAlgebraOptimizer[]
            {
                new RemoveNoRowSourcesOptimizer(),
                new RemoveUnusedColumnsOptimization(),
                new ReducedSelectOptimization()
            };
        }

        /// <summary>
        /// Creates the SQL algebra optimizers on the fly.
        /// </summary>        /// <returns>Optimization.ISqlAlgebraOptimizerOnTheFly[].</returns>
        public virtual ISqlAlgebraOptimizerOnTheFly[] CreateSqlAlgebraOptimizersOnTheFly()
        {
            return new ISqlAlgebraOptimizerOnTheFly[]
            {
                new IsNullOptimizer(),
                new ConcatenationInEqualConditionOptimizer(),
                new ConstantExprEqualityOptimizer()
            };
        }

        /// <summary>
        /// Creates the query context.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="schemaProvider"></param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="sparqlAlgebraOptimizerOnTheFly">The SPARQL algebra optimizers on the fly.</param>
        /// <param name="sqlAlgebraOptimizerOnTheFly">The SQL algebra optimizers on the fly.</param>
        public QueryContext CreateQueryContext(SparqlQuery originalQuery, MappingProcessor mapping, ISqlDb db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory, ISparqlAlgebraOptimizerOnTheFly[] sparqlAlgebraOptimizerOnTheFly, ISqlAlgebraOptimizerOnTheFly[] sqlAlgebraOptimizerOnTheFly)
        {
            return new QueryContext(originalQuery, mapping, db, schemaProvider, nodeFactory, sparqlAlgebraOptimizerOnTheFly, sqlAlgebraOptimizerOnTheFly);
        }
    }
}
