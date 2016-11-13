using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.PostProcess.Optimizers.IsNullOptimizerHelpers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers
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
        protected override IsNullOptimizerAnalyzeResult CreateInitialData(RelationalQuery query, IQueryContext context)
        {
            var analyzeResult = new IsNullOptimizerAnalyzeResult(query.Model);
            IsNullCalculator.TransformCalculusSource(query.Model, new IsNullCalculator.IsNullCalculatorParameter(analyzeResult, context));
            return analyzeResult;
        }

        /// <summary>
        /// Optimizes the relational query.
        /// </summary>
        /// <param name="relationalQuery">The relational query.</param>
        /// <param name="optimizationContext">The optimization context.</param>
        protected override RelationalQuery OptimizeRelationalQuery(RelationalQuery relationalQuery, OptimizationContext optimizationContext)
        {
            // TODO: Optimize value binders
            return base.OptimizeRelationalQuery(relationalQuery, optimizationContext);
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
            public void CheckPresentAnalysisResult(IsNullOptimizerAnalyzeResult analyzeResult, IQueryContext context)
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
