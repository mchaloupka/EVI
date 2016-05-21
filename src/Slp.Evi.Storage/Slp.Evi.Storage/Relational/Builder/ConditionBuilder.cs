using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using VDS.RDF;

namespace Slp.Evi.Storage.Relational.Builder
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
        public IFilterCondition CreateEqualsCondition(INode node, IValueBinder valueBinder, QueryContext context)
        {
            if (valueBinder is EmptyValueBinder)
            {
                return new AlwaysFalseCondition();
            }
            else if (valueBinder is BaseValueBinder)
            {
                var leftOperand = _expressionBuilder.CreateExpression(context, valueBinder);
                var rightOperand = _expressionBuilder.CreateExpression(context, node);

                return new EqualExpressionCondition(leftOperand, rightOperand);
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
        public IFilterCondition CreateIsBoundCondition(IValueBinder valueBinder, QueryContext context)
        {
            if (valueBinder is EmptyValueBinder)
            {
                return new AlwaysFalseCondition();
            }
            else if (valueBinder is BaseValueBinder)
            {
                var needed = valueBinder.NeededCalculusVariables;

                if (needed.Count() > 0)
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
            else if (valueBinder is CoalesceValueBinder)
            {
                var coalesceValueBinder = (CoalesceValueBinder) valueBinder;

                return new DisjunctionCondition(
                    coalesceValueBinder.ValueBinders.Select(x => CreateIsBoundCondition(x, context)).ToList());
            }
            else if (valueBinder is SwitchValueBinder)
            {
                var switchValueBinder = (SwitchValueBinder) valueBinder;

                return new DisjunctionCondition(switchValueBinder.Cases.Select(x => new ConjunctionCondition(new IFilterCondition[]
                {
                    new EqualExpressionCondition(new ColumnExpression(context, switchValueBinder.CaseVariable, false), new ConstantExpression(x.CaseValue, context)),
                    CreateIsBoundCondition(x.ValueBinder, context)
                })));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates the disjunction of the conditions
        /// </summary>
        /// <param name="context">The query context</param>
        /// <param name="conditions">The conditions, conjunction of every array member taken as the disjunction parameter</param>
        /// <returns></returns>
        public IFilterCondition CreateDisjunctionCondition(QueryContext context,
            params IFilterCondition[] conditions)
        {
            return new DisjunctionCondition(conditions);
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
        /// <param name="condition">The conditions</param>
        /// <param name="context">The query context.</param>
        /// <returns>The negation of the conditions.</returns>
        public IFilterCondition CreateNegationCondition(IFilterCondition condition,
            QueryContext context)
        {
            return new NegationCondition(condition);
        }

        /// <summary>
        /// Creates the equals conditions.
        /// </summary>
        /// <param name="firstValueBinder">The first value binder.</param>
        /// <param name="secondValueBinder">The second value binder.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateEqualsCondition(IValueBinder firstValueBinder, IValueBinder secondValueBinder, QueryContext context)
        {
            if (firstValueBinder is EmptyValueBinder)
            {
                return new NegationCondition(CreateIsBoundCondition(secondValueBinder, context));
            }
            else if (firstValueBinder is BaseValueBinder && secondValueBinder is BaseValueBinder)
            {
                var leftOperand = _expressionBuilder.CreateExpression(context, firstValueBinder);
                var rightOperand = _expressionBuilder.CreateExpression(context, secondValueBinder);
                return new EqualExpressionCondition(leftOperand, rightOperand);
            }
            else if (firstValueBinder is CoalesceValueBinder)
            {
                var disjunctionConditions = new List<IFilterCondition>();
                var binders = ((CoalesceValueBinder) firstValueBinder).ValueBinders.ToArray();

                for (int curIndex = 0; curIndex < binders.Length; curIndex++)
                {
                    var conjunctionConditions = new List<IFilterCondition>();

                    for (int prevIndex = 0; prevIndex < curIndex; prevIndex++)
                    {
                        conjunctionConditions.Add(new NegationCondition(CreateIsBoundCondition(binders[prevIndex], context)));
                    }

                    conjunctionConditions.Add(CreateEqualsCondition(binders[curIndex], secondValueBinder, context));
                    disjunctionConditions.Add(new DisjunctionCondition(conjunctionConditions));
                }

                return new DisjunctionCondition(disjunctionConditions);
            }
            else if (secondValueBinder is CoalesceValueBinder)
            {
                return CreateEqualsCondition(secondValueBinder, firstValueBinder, context);
            }
            else if (firstValueBinder is SwitchValueBinder)
            {
                var switchValueBinder = (SwitchValueBinder) firstValueBinder;

                return new DisjunctionCondition(switchValueBinder.Cases.Select(curCase => new ConjunctionCondition(new IFilterCondition[]
                {
                    new EqualExpressionCondition(new ColumnExpression(context, switchValueBinder.CaseVariable, false), new ConstantExpression(curCase.CaseValue, context)),
                    CreateEqualsCondition(curCase.ValueBinder, secondValueBinder, context)
                })).ToList());
            }
            else if (secondValueBinder is SwitchValueBinder)
            {
                return CreateEqualsCondition(secondValueBinder, firstValueBinder, context);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates the join equal condition
        /// </summary>
        /// <param name="valueBinder">First value binder</param>
        /// <param name="otherValueBinder">Other value binder</param>
        /// <param name="context">The query context</param>
        public IFilterCondition CreateJoinEqualCondition(IValueBinder valueBinder, IValueBinder otherValueBinder, QueryContext context)
        {
            return CreateDisjunctionCondition(context,
                CreateNegationCondition(CreateIsBoundCondition(valueBinder, context), context),
                CreateNegationCondition(CreateIsBoundCondition(otherValueBinder, context), context),
                CreateEqualsCondition(valueBinder, otherValueBinder, context));
        }
    }
}
