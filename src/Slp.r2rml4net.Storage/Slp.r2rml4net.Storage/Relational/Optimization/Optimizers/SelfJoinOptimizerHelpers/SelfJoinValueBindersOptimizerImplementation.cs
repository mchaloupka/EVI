using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.ValueBinders;
using Slp.r2rml4net.Storage.Relational.Utils;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// The implementation of the transformation <see cref="IValueBinder"/> to replace the variables
    /// </summary>
    public class SelfJoinValueBindersOptimizerImplementation 
        : IValueBinderVisitor
    {
        /// <summary>
        /// Visits <see cref="BaseValueBinder"/>
        /// </summary>
        /// <param name="baseValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            var optimizerData = (SelfJoinOptimizerData) data;

            if (baseValueBinder.NeededCalculusVariables.Any(x => optimizerData.IsReplaced(x)))
            {
                return new BaseValueBinder(baseValueBinder, 
                    (x) => optimizerData.IsReplaced(x) ? optimizerData.GetReplacingVariable(x) : x);
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
                var transformed = valueBinder.Accept(this, data);

                if (transformed != valueBinder)
                {
                    changed = true;
                }

                valueBinders.Add(valueBinder);
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
            var optimizerData = (SelfJoinOptimizerData)data;

            var caseVariable = switchValueBinder.CaseVariable;
            if (optimizerData.IsReplaced(caseVariable))
            {
                caseVariable = optimizerData.GetReplacingVariable(caseVariable);
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
    }
}
