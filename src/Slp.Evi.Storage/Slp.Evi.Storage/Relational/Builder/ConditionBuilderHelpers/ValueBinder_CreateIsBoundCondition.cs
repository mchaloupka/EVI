using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// Helper for <see cref="ConditionBuilder.CreateIsBoundCondition(Slp.Evi.Storage.Relational.Query.IValueBinder,Slp.Evi.Storage.Query.IQueryContext)"/>.
    /// </summary>
    public class ValueBinder_CreateIsBoundCondition
        : IValueBinderVisitor
    {
        /// <summary>
        /// The condition builder
        /// </summary>
        private readonly ConditionBuilder _conditionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBinder_CreateIsBoundCondition"/> class.
        /// </summary>
        /// <param name="conditionBuilder">The condition builder.</param>
        public ValueBinder_CreateIsBoundCondition(ConditionBuilder conditionBuilder)
        {
            _conditionBuilder = conditionBuilder;
        }

        /// <summary>
        /// Creates the is not null conditions.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateIsBoundCondition(IValueBinder valueBinder, IQueryContext context)
        {
            return (IFilterCondition) valueBinder.Accept(this, context);
        }

        /// <summary>
        /// Visits <see cref="BaseValueBinder"/>
        /// </summary>
        /// <param name="baseValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            var needed = baseValueBinder.NeededCalculusVariables;

            if (needed.Any())
            {
                List<IFilterCondition> conditions = new List<IFilterCondition>();

                foreach (var calculusVariable in needed)
                {
                    conditions.Add(new NegationCondition(new IsNullCondition(calculusVariable)));
                }

                return new ConjunctionCondition(conditions);
            }
            else
            {
                return new AlwaysTrueCondition();
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
            return new AlwaysFalseCondition();
        }

        /// <summary>
        /// Visits <see cref="CoalesceValueBinder"/>
        /// </summary>
        /// <param name="coalesceValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            return new DisjunctionCondition(
                coalesceValueBinder.ValueBinders.Select(x => x.Accept(this, data)).Cast<IFilterCondition>().ToList());
        }

        /// <summary>
        /// Visits <see cref="SwitchValueBinder"/>
        /// </summary>
        /// <param name="switchValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            var context = (IQueryContext) data;

            return new DisjunctionCondition(switchValueBinder.Cases.Select(x => new ConjunctionCondition(new IFilterCondition[]
            {
                    new ComparisonCondition(new ColumnExpression(switchValueBinder.CaseVariable, false), new ConstantExpression(x.CaseValue, context), ComparisonTypes.EqualTo),
                    (IFilterCondition)x.ValueBinder.Accept(this, data)
            })));
        }

        /// <summary>
        /// Visits <see cref="ExpressionSetValueBinder"/>
        /// </summary>
        /// <param name="expressionSetValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExpressionSetValueBinder expressionSetValueBinder, object data)
        {
            return _conditionBuilder.CreateIsBoundCondition(expressionSetValueBinder.Expression, (IQueryContext) data);
        }
    }
}