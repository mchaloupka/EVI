using System;
using System.Text;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.IsNullOptimizerHelpers;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers
{
    /// <summary>
    /// The optimizer of <c>IS NULL</c> statements
    /// </summary>
    public class IsNullOptimizer
        : BaseRelationalOptimizer<IsNullOptimizerAnalyzeResult>
    {
        /// <summary>
        /// The <see cref="IsNullOptimizerHelpers.IsNullCalculator"/> instance for this optimizer
        /// </summary>
        private IsNullCalculator IsNullCalculator => ((IsNullOptimizerImplementation) OptimizerImplementation).IsNullCalculator;

        /// <summary>
        /// Constructs an instance of <see cref="IsNullOptimizer"/>
        /// </summary>
        public IsNullOptimizer()
            : base(new IsNullOptimizerImplementation(new IsNullCalculator()))
        { }

        /// <summary>
        /// Creates the initial data.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="context"></param>
        protected override IsNullOptimizerAnalyzeResult CreateInitialData(RelationalQuery query, QueryContext context)
        {
            var analyzeResult = new IsNullOptimizerAnalyzeResult(query.Model);
            IsNullCalculator.TransformCalculusSource(query.Model, new IsNullCalculator.IsNullCalculatorParameter(analyzeResult, context));
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
        /// Processes the visit of <see cref="CalculusModel" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected override ICalculusSource ProcessVisit(CalculusModel toVisit, OptimizationContext data)
        {
            data.Data.PushCurrentSource(toVisit);
            var result = base.ProcessVisit(toVisit, data);
            data.Data.PopCurrentSource();
            return result;
        }

        /// <summary>
        /// Processes the visit of <see cref="SqlTable" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected override ICalculusSource ProcessVisit(SqlTable toVisit, OptimizationContext data)
        {
            data.Data.PushCurrentSource(toVisit);
            var result = base.ProcessVisit(toVisit, data);
            data.Data.PopCurrentSource();
            return result;
        }

        /// <summary>
        /// Processes the visit of <see cref="NegationCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected override IFilterCondition ProcessVisit(NegationCondition toVisit, OptimizationContext data)
        {
            data.Data.EnterNegationCondition();
            var result = base.ProcessVisit(toVisit, data);
            data.Data.LeaveNegationCondition();
            return result;
        }

        /// <summary>
        /// The implementation of <see cref="IsNullOptimizer"/>
        /// </summary>
        private class IsNullOptimizerImplementation
            : BaseRelationalOptimizerImplementation<IsNullOptimizerAnalyzeResult>
        {
            /// <summary>
            /// The <see cref="IsNullOptimizerHelpers.IsNullCalculator"/> instance for this optimizer
            /// </summary>
            private readonly IsNullCalculator _isNullCalculator;

            /// <summary>
            /// Initializes a new instance of the <see cref="IsNullOptimizerImplementation"/> class.
            /// </summary>
            /// <param name="isNullCalculator">The is null calculator.</param>
            public IsNullOptimizerImplementation(IsNullCalculator isNullCalculator)
            {
                _isNullCalculator = isNullCalculator;
            }

            /// <summary>
            /// The <see cref="IsNullOptimizerHelpers.IsNullCalculator"/> instance for this optimizer
            /// </summary>
            public IsNullCalculator IsNullCalculator => _isNullCalculator;

            /// <summary>
            /// Checks whether the analysis result is present.
            /// </summary>
            /// <param name="analyzeResult">The analysis result.</param>
            /// <param name="context">The query context</param>
            public void CheckPresentAnalysisResult(IsNullOptimizerAnalyzeResult analyzeResult, QueryContext context)
            {
                var source = analyzeResult.CurrentSource;

                if (!analyzeResult.HasValueForSource(source))
                {
                    var newAnalyzeResult = new IsNullOptimizerAnalyzeResult(source);
                    _isNullCalculator.TransformCalculusSource(source, new IsNullCalculator.IsNullCalculatorParameter(newAnalyzeResult, context));
                    newAnalyzeResult.CopyTo(analyzeResult);
                }
            }

            /// <summary>
            /// Process the <see cref="IsNullCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(IsNullCondition toTransform, OptimizationContext data)
            {
                CheckPresentAnalysisResult(data.Data, data.Context);

                var analysisData = data.Data.GetCurrentValue();

                if (analysisData.IsInNotNullConditions(toTransform.Variable))
                {
                    if (!analysisData.IsInNotNullConditions(toTransform))
                    {
                        return new AlwaysFalseCondition();
                    }
                }
                else if (analysisData.IsInNullConditions(toTransform.Variable))
                {
                    if (!analysisData.IsInNullConditions(toTransform))
                    {
                        return new AlwaysTrueCondition();
                    }
                }

                return toTransform;
            }
        }
    }
}
