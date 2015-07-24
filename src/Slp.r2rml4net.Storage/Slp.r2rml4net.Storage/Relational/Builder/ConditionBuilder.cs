using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.ValueBinders;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Relational.Builder
{
    /// <summary>
    /// The conditions builder
    /// </summary>
    public class ConditionBuilder
    {
        /// <summary>
        /// The expression builder
        /// </summary>
        private readonly ExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionBuilder"/> class.
        /// </summary>
        /// <param name="expressionBuilder">The expression builder.</param>
        public ConditionBuilder(ExpressionBuilder expressionBuilder)
        {
            _expressionBuilder = expressionBuilder;
        }

        /// <summary>
        /// Creates the equals conditions.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="valueBinder">The value binder.</param>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable&lt;ICondition&gt;.</returns>
        public IEnumerable<IFilterCondition> CreateEqualsConditions(INode node, IValueBinder valueBinder, QueryContext context)
        {
            if (valueBinder is EmptyValueBinder)
            {
                yield return new AlwaysFalseCondition();
            }
            else if (valueBinder is BaseValueBinder)
            {
                var leftOperand = _expressionBuilder.CreateExpression(context, valueBinder);
                var rightOperand = _expressionBuilder.CreateExpression(context, node);

                yield return new EqualExpressionCondition(leftOperand, rightOperand);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates the is not null conditions.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        /// <param name="context">The context.</param>
        public IEnumerable<IFilterCondition> CreateIsBoundConditions(IValueBinder valueBinder, QueryContext context)
        {
            if (valueBinder is EmptyValueBinder)
            {
                yield return new AlwaysFalseCondition();
            }
            else if (valueBinder is BaseValueBinder)
            {
                var needed = valueBinder.NeededCalculusVariables;

                if (needed.Count() > 0)
                {
                    foreach (var calculusVariable in needed)
                    {
                        yield return new NegationCondition(new IsNullCondition(calculusVariable));
                    }
                }
                else
                {
                    yield return new AlwaysTrueCondition();
                }
            }
        }

        /// <summary>
        /// Creates the disjunction of the conditions
        /// </summary>
        /// <param name="context">The query context</param>
        /// <param name="conditions">The conditions, conjunction of every array member taken as the disjunction parameter</param>
        /// <returns></returns>
        public IEnumerable<IFilterCondition> CreateDisjunctionConditions(QueryContext context,
            params IEnumerable<IFilterCondition>[] conditions)
        {
            yield return new DisjunctionCondition(conditions.Select(x => CreateConjunctionCondition(x, context)));
        }

        /// <summary>
        /// Creates the conjunction of the conditions.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        /// <param name="context">The query context.</param>
        /// <returns></returns>
        public IFilterCondition CreateConjunctionCondition(IEnumerable<IFilterCondition> conditions,
            QueryContext context)
        {
            return new ConjunctionCondition(conditions);
        }

        /// <summary>
        /// Create negation conditions
        /// </summary>
        /// <param name="conditions">The conditions</param>
        /// <param name="context">The query context.</param>
        /// <returns>The negation of the conditions.</returns>
        public IEnumerable<IFilterCondition> CreateNegationConditions(IEnumerable<IFilterCondition> conditions,
            QueryContext context)
        {
            return conditions.Select(x => new NegationCondition(x));
        }

        /// <summary>
        /// Creates the equals conditions.
        /// </summary>
        /// <param name="firstValueBinder">The first value binder.</param>
        /// <param name="secondValueBinder">The second value binder.</param>
        /// <param name="context">The context.</param>
        public IEnumerable<IFilterCondition> CreateEqualsConditions(IValueBinder firstValueBinder, IValueBinder secondValueBinder, QueryContext context)
        {
            if (firstValueBinder is EmptyValueBinder)
            {
                foreach (var condition in CreateIsBoundConditions(secondValueBinder, context).Select(x => new NegationCondition(x)))
                {
                    yield return condition;
                }
            }
            else if (firstValueBinder is BaseValueBinder && secondValueBinder is BaseValueBinder)
            {
                var leftOperand = _expressionBuilder.CreateExpression(context, firstValueBinder);
                var rightOperand = _expressionBuilder.CreateExpression(context, secondValueBinder);
                yield return new EqualExpressionCondition(leftOperand, rightOperand);
            }
        }

        /// <summary>
        /// Creates the join equal condition
        /// </summary>
        /// <param name="valueBinder">First value binder</param>
        /// <param name="otherValueBinder">Other value binder</param>
        /// <param name="context">The query context</param>
        public IEnumerable<IFilterCondition> CreateJoinEqualCondition(IValueBinder valueBinder, IValueBinder otherValueBinder, QueryContext context)
        {
            return CreateDisjunctionConditions(context,
                CreateNegationConditions(CreateIsBoundConditions(valueBinder, context), context),
                CreateNegationConditions(CreateIsBoundConditions(otherValueBinder, context), context),
                CreateEqualsConditions(valueBinder, otherValueBinder, context));
        }
    }
}
