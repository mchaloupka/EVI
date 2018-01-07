using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.ValueBinders;

namespace Slp.Evi.Storage.Relational.Builder.ValueBinderHelpers
{
    public class ValueBinderFlattener
        : IValueBinderVisitor
    {
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
            return new(IFilterCondition, BaseValueBinder)[]
            {
                (new AlwaysTrueCondition(), baseValueBinder)
            };
        }

        /// <inheritdoc />
        public object Visit(EmptyValueBinder emptyValueBinder, object data)
        {
            return new(IFilterCondition, BaseValueBinder)[0];
        }

        /// <inheritdoc />
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(ExpressionSetValueBinder expressionSetValueBinder, object data)
        {
            throw new NotImplementedException();
        }
    }
}
