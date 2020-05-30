using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.ValueBinders;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers.IsNullOptimizerHelpers
{
    /// <summary>
    /// Optimizer of value binders for <see cref="IsNullOptimizer"/>.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.ValueBinders.IValueBinderVisitor" />
    public class IsNullValueBinderOptimizer
        : IValueBinderVisitor
    {
        private readonly IsNullOptimizer.IsNullOptimizerImplementation _optimizerImplementation;
        private readonly IsAlwaysBoundChecker _isAlwaysBoundChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsNullValueBinderOptimizer"/> class.
        /// </summary>
        /// <param name="optimizerImplementation">The optimizer implementation.</param>
        public IsNullValueBinderOptimizer(IsNullOptimizer.IsNullOptimizerImplementation optimizerImplementation)
        {
            _optimizerImplementation = optimizerImplementation;
            _isAlwaysBoundChecker = new IsAlwaysBoundChecker(_optimizerImplementation);
        }

        /// <inheritdoc />
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            var context = (BaseRelationalOptimizer<IsNullOptimizerAnalyzeResult>.OptimizationContext)data;
            _optimizerImplementation.CheckPresentAnalysisResult(context.Data, context.Context);
            var analyzis = context.Data.GetCurrentValue();

            if (baseValueBinder.NeededCalculusVariables.Any(x => analyzis.IsInNullConditions(x)))
            {
                return new EmptyValueBinder(baseValueBinder.VariableName);
            }

            return baseValueBinder;
        }

        /// <inheritdoc />
        public object Visit(EmptyValueBinder emptyValueBinder, object data)
        {
            return emptyValueBinder;
        }

        /// <inheritdoc />
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            var binders = new List<IValueBinder>();
            bool changed = false;

            for (var index = 0; index < coalesceValueBinder.ValueBinders.Length; index++)
            {
                var valueBinder = coalesceValueBinder.ValueBinders[index];
                var newValueBinder = (IValueBinder)valueBinder.Accept(this, data);

                if (newValueBinder is EmptyValueBinder)
                {
                    changed = true;
                    continue;
                }
                else if (newValueBinder != valueBinder)
                {
                    changed = true;
                }

                binders.Add(newValueBinder);

                if (index < coalesceValueBinder.ValueBinders.Length - 1 && (bool)newValueBinder.Accept(_isAlwaysBoundChecker, data))
                {
                    changed = true;
                    break;
                }
            }

            if (binders.Count == 0)
            {
                return new EmptyValueBinder(coalesceValueBinder.VariableName);
            }
            else if (binders.Count == 1)
            {
                return binders[0];
            }
            else if (changed)
            {
                return new CoalesceValueBinder(coalesceValueBinder.VariableName, binders.ToArray());
            }
            else
            {
                return coalesceValueBinder;
            }
        }

        /// <inheritdoc />
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            var cases = new List<SwitchValueBinder.Case>();
            bool changed = false;

            foreach (var @case in switchValueBinder.Cases)
            {
                var newValueBinder = (IValueBinder) @case.ValueBinder.Accept(this, data);

                if (newValueBinder != @case.ValueBinder)
                {
                    changed = true;
                }

                cases.Add(new SwitchValueBinder.Case(@case.CaseValue, newValueBinder));
            }

            if (changed)
            {
                return new SwitchValueBinder(switchValueBinder.VariableName, switchValueBinder.CaseVariable, cases);
            }
            else
            {
                return switchValueBinder;
            }
        }

        /// <inheritdoc />
        public object Visit(ExpressionSetValueBinder expressionSetValueBinder, object data)
        {
            var context = (BaseRelationalOptimizer<IsNullOptimizerAnalyzeResult>.OptimizationContext) data;

            var newExpressionSet = _optimizerImplementation.TransformExpressionSet(expressionSetValueBinder.ExpressionSet, context);
            if (newExpressionSet != expressionSetValueBinder.ExpressionSet)
            {
                return new ExpressionSetValueBinder(expressionSetValueBinder.VariableName, newExpressionSet);
            }
            else
            {
                return expressionSetValueBinder;
            }
        }
    }
}
