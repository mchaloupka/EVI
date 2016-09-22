using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// The implementation of the transformation <see cref="IValueBinder"/> to replace the variables
    /// </summary>
    public class SelfJoinValueBindersOptimizerImplementation 
        : IValueBinderVisitor
    {
        /// <summary>
        /// The optimizer implementation
        /// </summary>
        private readonly BaseRelationalOptimizerImplementation<SelfJoinOptimizerData> _optimizerImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfJoinValueBindersOptimizerImplementation"/> class.
        /// </summary>
        /// <param name="optimizerImplementation">The optimizer implementation.</param>
        public SelfJoinValueBindersOptimizerImplementation(BaseRelationalOptimizerImplementation<SelfJoinOptimizerData> optimizerImplementation)
        {
            _optimizerImplementation = optimizerImplementation;
        }

        /// <summary>
        /// Visits <see cref="BaseValueBinder"/>
        /// </summary>
        /// <param name="baseValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            var optimizerData = (BaseRelationalOptimizer<SelfJoinOptimizerData>.OptimizationContext) data;

            if (baseValueBinder.NeededCalculusVariables.Any(x => optimizerData.Data.IsReplaced(x)))
            {
                return new BaseValueBinder(baseValueBinder, 
                    (x) => optimizerData.Data.IsReplaced(x) ? optimizerData.Data.GetReplacingVariable(x) : x);
            }
            else
            {
                return baseValueBinder;
            }
        }

        /// <summary>
        /// Visits <see cref="EmptyValueBinder"/>
        /// </summary>
        /// <param name="emptyValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyValueBinder emptyValueBinder, object data)
        {
            return emptyValueBinder;
        }

        /// <summary>
        /// Visits <see cref="CoalesceValueBinder"/>
        /// </summary>
        /// <param name="coalesceValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            var valueBinders = new List<IValueBinder>();

            var changed = false;

            foreach (var valueBinder in coalesceValueBinder.ValueBinders)
            {
                var transformed = (IValueBinder)valueBinder.Accept(this, data);

                if (transformed != valueBinder)
                {
                    changed = true;
                }

                valueBinders.Add(transformed);
            }

            if (changed)
            {
                return new CoalesceValueBinder(coalesceValueBinder.VariableName, valueBinders.ToArray());
            }
            else
            {
                return coalesceValueBinder;
            }
        }

        /// <summary>
        /// Visits <see cref="SwitchValueBinder"/>
        /// </summary>
        /// <param name="switchValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            var changed = false;
            var optimizerData = (BaseRelationalOptimizer<SelfJoinOptimizerData>.OptimizationContext)data;

            var caseVariable = switchValueBinder.CaseVariable;
            if (optimizerData.Data.IsReplaced(caseVariable))
            {
                caseVariable = optimizerData.Data.GetReplacingVariable(caseVariable);
                changed = true;
            }

            var cases = new List<SwitchValueBinder.Case>();

            foreach (var @case in switchValueBinder.Cases)
            {
                var transformed = (IValueBinder)@case.ValueBinder.Accept(this, optimizerData);

                if (transformed != @case.ValueBinder)
                {
                    changed = true;
                }

                cases.Add(new SwitchValueBinder.Case(@case.CaseValue, transformed));
            }

            if (changed)
            {
                return new SwitchValueBinder(switchValueBinder.VariableName, caseVariable, cases);
            }
            else
            {
                return switchValueBinder;
            }
        }

        /// <summary>
        /// Visits <see cref="ExpressionValueBinder"/>
        /// </summary>
        /// <param name="expressionValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExpressionValueBinder expressionValueBinder, object data)
        {
            var newExpression = _optimizerImplementation.TransformExpression(expressionValueBinder.Expression,
                (BaseRelationalOptimizer<SelfJoinOptimizerData>.OptimizationContext)data);

            if (newExpression != expressionValueBinder.Expression)
            {
                return new ExpressionValueBinder(expressionValueBinder.VariableName, newExpression);
            }
            else
            {
                return expressionValueBinder;
            }
        }
    }
}
