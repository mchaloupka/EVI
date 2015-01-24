using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Bootstrap
{
    /// <summary>
    /// Class DefaultR2RMLStorageFactory.
    /// </summary>
    public class DefaultR2RMLStorageFactory : IR2RMLStorageFactory
    {
        /// <summary>
        /// Creates the query processor.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="mapping">The mapping.</param>
        public virtual Query.QueryProcessor CreateQueryProcessor(Sql.ISqlDb db, TCode.r2rml4net.IR2RML mapping)
        {
            return new Query.QueryProcessor(db, mapping, this);
        }

        /// <summary>
        /// Creates the mapping processor.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        public virtual Mapping.MappingProcessor CreateMappingProcessor(TCode.r2rml4net.IR2RML mapping)
        {
            return new Mapping.MappingProcessor(mapping);
        }

        /// <summary>
        /// Creates the sparql algebra builder.
        /// </summary>
        public virtual Sparql.SparqlAlgebraBuilder CreateSparqlAlgebraBuilder()
        {
            return new Sparql.SparqlAlgebraBuilder();
        }

        /// <summary>
        /// Creates the SQL algebra builder.
        /// </summary>
        public virtual Sql.SqlAlgebraBuilder CreateSqlAlgebraBuilder()
        {
            return new Sql.SqlAlgebraBuilder();
        }

        /// <summary>
        /// Creates the sparql algebra optimizers.
        /// </summary>
        public virtual Optimization.ISparqlAlgebraOptimizer[] CreateSparqlAlgebraOptimizers()
        {
            return new Optimization.ISparqlAlgebraOptimizer[]
            {
                new Optimization.SparqlAlgebra.R2RMLOptimizer(),
                new Optimization.SparqlAlgebra.UnionOptimizer(),
                new Optimization.SparqlAlgebra.JoinOptimizer(),
                new Optimization.SparqlAlgebra.SelectIntoUnionOptimizer()
            };
        }

        /// <summary>
        /// Creates the sparql algebra optimizers on the fly.
        /// </summary>
        public virtual Optimization.ISparqlAlgebraOptimizerOnTheFly[] CreateSparqlAlgebraOptimizersOnTheFly()
        {
            return new Optimization.ISparqlAlgebraOptimizerOnTheFly[]
            {

            };
        }

        /// <summary>
        /// Creates the SQL optimizers.
        /// </summary>
        public virtual Optimization.ISqlAlgebraOptimizer[] CreateSqlOptimizers()
        {
            return new Slp.r2rml4net.Storage.Optimization.ISqlAlgebraOptimizer[]
            {
                new Optimization.SqlAlgebra.RemoveNoRowSourcesOptimizer(),
                new Optimization.SqlAlgebra.RemoveUnusedColumnsOptimization(),
                new Optimization.SqlAlgebra.ReducedSelectOptimization()
            };
        }

        /// <summary>
        /// Creates the SQL algebra optimizers on the fly.
        /// </summary>        /// <returns>Optimization.ISqlAlgebraOptimizerOnTheFly[].</returns>
        public virtual Optimization.ISqlAlgebraOptimizerOnTheFly[] CreateSqlAlgebraOptimizersOnTheFly()
        {
            return new Slp.r2rml4net.Storage.Optimization.ISqlAlgebraOptimizerOnTheFly[]
            {
                new Optimization.SqlAlgebra.IsNullOptimizer(),
                new Optimization.SqlAlgebra.ConcatenationInEqualConditionOptimizer(),
                new Optimization.SqlAlgebra.ConstantExprEqualityOptimizer()
            };
        }

        /// <summary>
        /// Creates the query context.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="sparqlAlgebraOptimizerOnTheFly">The SPARQL algebra optimizers on the fly.</param>
        /// <param name="sqlAlgebraOptimizerOnTheFly">The SQL algebra optimizers on the fly.</param>
        public Query.QueryContext CreateQueryContext(VDS.RDF.Query.SparqlQuery originalQuery, Mapping.MappingProcessor mapping, Sql.ISqlDb db, VDS.RDF.INodeFactory nodeFactory, Optimization.ISparqlAlgebraOptimizerOnTheFly[] sparqlAlgebraOptimizerOnTheFly, Optimization.ISqlAlgebraOptimizerOnTheFly[] sqlAlgebraOptimizerOnTheFly)
        {
            return new Query.QueryContext(originalQuery, mapping, db, nodeFactory, sparqlAlgebraOptimizerOnTheFly, sqlAlgebraOptimizerOnTheFly);
        }
    }
}
