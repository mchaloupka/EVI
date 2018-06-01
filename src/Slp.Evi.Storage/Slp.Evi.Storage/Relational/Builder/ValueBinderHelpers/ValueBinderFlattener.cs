using System.Collections.Generic;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;

namespace Slp.Evi.Storage.Relational.Builder.ValueBinderHelpers
{
    /// <summary>
    /// The flattener for <see cref="IValueBinder"/>.
    /// </summary>
    public class ValueBinderFlattener
        : IValueBinderVisitor
    {
        private readonly ConditionBuilder _conditionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBinderFlattener"/> class.
        /// </summary>
        public ValueBinderFlattener(ConditionBuilder conditionBuilder)
        {
            _conditionBuilder = conditionBuilder;
        }

        /// <summary>
        /// Gets the flat value binder representation.
        /// </summary>
        /// <param name="valueBinder">The value binder to flatten.</param>
        /// <param name="queryContext">The query context.</param>
        /// <returns>Enumeration of all possible base or expression value binders with their conditions.</returns>
        public IEnumerable<(IFilterCondition condition, IValueBinder valueBinder)> Flatten(IValueBinder valueBinder, IQueryContext queryContext)
        {
            return (IEnumerable<(IFilterCondition condition, IValueBinder valueBinder)>) valueBinder.Accept(this, queryContext);
        }

        /// <inheritdoc />
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            return new(IFilterCondition, IValueBinder)[]
            {
                (new AlwaysTrueCondition(), baseValueBinder)
            };
        }

        /// <inheritdoc />
        public object Visit(EmptyValueBinder emptyValueBinder, object data)
        {
            return new(IFilterCondition, IValueBinder)[0];
        }

        /// <inheritdoc />
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            return FlattenCoalesceValueBinder(coalesceValueBinder, (QueryContext) data);
        }

        private IEnumerable<(IFilterCondition, IValueBinder)> FlattenCoalesceValueBinder(CoalesceValueBinder coalesceValueBinder, QueryContext queryContext)
        {
            IFilterCondition condition = new AlwaysTrueCondition();

            foreach (var valueBinder in coalesceValueBinder.ValueBinders)
            {
                var flattened = Flatten(valueBinder, queryContext);

                foreach (var combination in CombineFlattened(flattened, condition))
                {
                    yield return combination;
                }

                condition = new ConjunctionCondition(new[]
                {
                    condition,
                    new NegationCondition(_conditionBuilder.CreateIsBoundCondition(valueBinder, queryContext))
                });
            }
        }

        private static IEnumerable<(IFilterCondition, IValueBinder)> CombineFlattened(IEnumerable<(IFilterCondition condition, IValueBinder valueBinder)> flattened, IFilterCondition condition)
        {
            foreach (var valueTuple in flattened)
            {
                yield return (new ConjunctionCondition(new[] {valueTuple.condition, condition}),
                    valueTuple.valueBinder);
            }
        }

        /// <inheritdoc />
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            return FlattenSwitchValueBinder(switchValueBinder, (QueryContext) data);
        }

        private IEnumerable<(IFilterCondition condition, IValueBinder valueBinder)> FlattenSwitchValueBinder(SwitchValueBinder switchValueBinder, QueryContext queryContext)
        {
            var caseColumn = new ColumnExpression(switchValueBinder.CaseVariable, false);

            foreach (var @case in switchValueBinder.Cases)
            {
                var flattened = Flatten(@case.ValueBinder, queryContext);

                foreach (var combination in CombineFlattened(flattened,
                    new ComparisonCondition(caseColumn, new ConstantExpression(@case.CaseValue, queryContext), ComparisonTypes.EqualTo)))
                {
                    yield return combination;
                }
            }
        }

        /// <inheritdoc />
        public object Visit(ExpressionSetValueBinder expressionSetValueBinder, object data)
        {
            return new(IFilterCondition, IValueBinder)[]
            {
                (new AlwaysTrueCondition(), expressionSetValueBinder)
            };
        }
    }
}

