using System;
using System.Text;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.IsNullOptimizerHelpers;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers
{
    /// <summary>
    /// The optimizer of <c>IS NULL</c> statements
    /// </summary>
    public class IsNullOptimizer
        : BaseRelationalOptimizer<IsNullOptimizerAnalyzeResult>
    {
        private IsNullCalculator _isNullCalculator;

        /// <summary>
        /// Constructs an instance of <see cref="IsNullOptimizer"/>
        /// </summary>
        public IsNullOptimizer()
            : base(new IsNullOptimizerImplementation())
        {
            _isNullCalculator = new IsNullCalculator();
        }

        /// <summary>
        /// Creates the initial data.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="context"></param>
        protected override IsNullOptimizerAnalyzeResult CreateInitialData(RelationalQuery query, QueryContext context)
        {
            var analyzeResult = new IsNullOptimizerAnalyzeResult(query.Model);
            _isNullCalculator.TransformCalculusSource(query.Model, analyzeResult);
            return analyzeResult;
        }

        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        public override RelationalQuery Optimize(RelationalQuery query, QueryContext context)
        {
            var result = base.Optimize(query, context);

            // TODO: Optimize value binders

            return result;
        }

        /// <summary>
        /// The implementation of <see cref="IsNullOptimizer"/>
        /// </summary>
        private class IsNullOptimizerImplementation
            : BaseRelationalOptimizerImplementation<IsNullOptimizerAnalyzeResult>
        {
            
        }
    }
}
