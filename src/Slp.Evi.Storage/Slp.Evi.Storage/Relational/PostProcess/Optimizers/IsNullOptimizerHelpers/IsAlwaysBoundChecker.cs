using System.Linq;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.ValueBinders;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers.IsNullOptimizerHelpers
{
    /// <summary>
    /// The checker whether a value binder is always bound (according to "is null" conditions)
    /// </summary>
    public class IsAlwaysBoundChecker
        : IValueBinderVisitor
    {
        private readonly IsNullOptimizer.IsNullOptimizerImplementation _optimizerImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsAlwaysBoundChecker"/> class.
        /// </summary>
        /// <param name="optimizerImplementation">The optimizer implementation.</param>
        public IsAlwaysBoundChecker(IsNullOptimizer.IsNullOptimizerImplementation optimizerImplementation)
        {
            _optimizerImplementation = optimizerImplementation;
        }

        /// <inheritdoc />
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            var context = (BaseRelationalOptimizer<IsNullOptimizerAnalyzeResult>.OptimizationContext)data;
            _optimizerImplementation.CheckPresentAnalysisResult(context.Data, context.Context);
            var analyzis = context.Data.GetCurrentValue();

            return baseValueBinder.NeededCalculusVariables.All(x => analyzis.IsInNotNullConditions(x));
        }

        /// <inheritdoc />
        public object Visit(EmptyValueBinder emptyValueBinder, object data)
        {
            return false;
        }

        /// <inheritdoc />
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            // The coalesce value binder is already optimized, so if it is always bound, it should be always bound on the last item
            return coalesceValueBinder.ValueBinders.Last().Accept(this, data);
        }

        /// <inheritdoc />
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            return false;
        }

        /// <inheritdoc />
        public object Visit(ExpressionSetValueBinder expressionSetValueBinder, object data)
        {
            return false;
        }
    }
}