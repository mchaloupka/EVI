using System;
using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Binders;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql
{
    /// <summary>
    /// Condition builder.
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
        /// Creates the equals condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="node">The node.</param>
        /// <param name="valueBinder">The value binder.</param>
        /// <returns>The created condition.</returns>
        /// <exception cref="System.NotImplementedException">Not supported value binder in method CreateEqualsCondition</exception>
        public ICondition CreateEqualsCondition(QueryContext context, INode node, IBaseValueBinder valueBinder)
        {
            if (valueBinder is ValueBinder)
            {
                var leftOperand = _expressionBuilder.CreateExpression(context, (ValueBinder)valueBinder);
                var rightOperand = _expressionBuilder.CreateExpression(context, node);

                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if (valueBinder is CoalesceValueBinder)
            {
                var orCondition = new OrCondition();
                var binders = ((CoalesceValueBinder)valueBinder).InnerBinders.ToArray();

                for (int curIndex = 0; curIndex < binders.Length; curIndex++)
                {
                    var andCondition = new AndCondition();

                    for (int prevIndex = 0; prevIndex < curIndex; prevIndex++)
                    {
                        andCondition.AddToCondition(CreateIsNullCondition(context, binders[prevIndex]));
                    }

                    andCondition.AddToCondition(CreateEqualsCondition(context, node, binders[curIndex]));
                    orCondition.AddToCondition(andCondition);
                }

                return orCondition;
            }
            else if (valueBinder is CaseValueBinder)
            {
                var orCondition = new OrCondition();
                var statements = ((CaseValueBinder)valueBinder).Statements;

                foreach (var statement in statements)
                {
                    var andCondition = new AndCondition();
                    andCondition.AddToCondition(statement.Condition);
                    andCondition.AddToCondition(CreateEqualsCondition(context, node, statement.ValueBinder));
                    orCondition.AddToCondition(andCondition);
                }

                return orCondition;
            }
            else if (valueBinder is SqlSideValueBinder)
            {
                var sqlSideValueBinder = (SqlSideValueBinder)valueBinder;
                var column = sqlSideValueBinder.Column;

                var leftOperand = _expressionBuilder.CreateColumnExpression(context, column, false);
                var rightOperand = _expressionBuilder.CreateExpression(context, node);

                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if (valueBinder is ExpressionValueBinder)
            {
                var leftOperand = (IExpression)((ExpressionValueBinder)valueBinder).Expression.Clone();
                var rightOperand = _expressionBuilder.CreateExpression(context, node);
                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if(valueBinder is BlankValueBinder)
            {
                return new AlwaysFalseCondition();
            }
            else
                throw new NotImplementedException("Not supported value binder in method CreateEqualsCondition");
        }

        /// <summary>
        /// Creates the equals condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="firstValBinder">The first value binder.</param>
        /// <param name="secondValBinder">The second value binder.</param>
        /// <returns>The created condition.</returns>
        /// <exception cref="System.Exception">Unknown value binder</exception>
        public ICondition CreateEqualsCondition(QueryContext context, IBaseValueBinder firstValBinder, IBaseValueBinder secondValBinder)
        {
            if (firstValBinder is ValueBinder && secondValBinder is ValueBinder)
            {
                var leftOperand = _expressionBuilder.CreateExpression(context, (ValueBinder)firstValBinder);
                var rightOperand = _expressionBuilder.CreateExpression(context, (ValueBinder)secondValBinder);
                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if (firstValBinder is CoalesceValueBinder)
            {
                var orCondition = new OrCondition();
                var binders = ((CoalesceValueBinder)firstValBinder).InnerBinders.ToArray();

                for (int curIndex = 0; curIndex < binders.Length; curIndex++)
                {
                    var andCondition = new AndCondition();

                    for (int prevIndex = 0; prevIndex < curIndex; prevIndex++)
                    {
                        andCondition.AddToCondition(CreateIsNullCondition(context, binders[prevIndex]));
                    }

                    andCondition.AddToCondition(CreateEqualsCondition(context, binders[curIndex], secondValBinder));
                    orCondition.AddToCondition(andCondition);
                }

                return orCondition;
            }
            else if(firstValBinder is CaseValueBinder)
            {
                var statements = ((CaseValueBinder)firstValBinder).Statements.ToArray();
                var orCondition = new OrCondition();

                foreach (var statement in statements)
                {
                    var andCondition = new AndCondition();
                    andCondition.AddToCondition(statement.Condition);
                    andCondition.AddToCondition(CreateEqualsCondition(context, statement.ValueBinder, secondValBinder));
                    orCondition.AddToCondition(andCondition);
                }

                return orCondition;
            }
            else if (firstValBinder is ValueBinder)
            {
                return CreateIsNullCondition(context, secondValBinder);
            }
            else if (secondValBinder is CoalesceValueBinder)
            {
                return CreateEqualsCondition(context, secondValBinder, firstValBinder);
            }
            else if (secondValBinder is CaseValueBinder)
            {
                return CreateEqualsCondition(context, secondValBinder, firstValBinder);
            }
            else
                throw new Exception("Unknown value binder");
        }

        /// <summary>
        /// Creates the equals condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="column">The column.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateEqualsCondition(QueryContext context, ISqlColumn column, IExpression expression)
        {
            return new EqualsCondition(_expressionBuilder.CreateColumnExpression(context, column, false), expression);
        }

        /// <summary>
        /// Creates the equals condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="firstCol">The first column.</param>
        /// <param name="secondCol">The second column.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateEqualsCondition(QueryContext context, ISqlColumn firstCol, ISqlColumn secondCol)
        {
            return new EqualsCondition(_expressionBuilder.CreateColumnExpression(context, firstCol, false), _expressionBuilder.CreateColumnExpression(context, secondCol, false));
        }

        /// <summary>
        /// Creates the always true condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateAlwaysTrueCondition(QueryContext context)
        {
            return new AlwaysTrueCondition();
        }

        /// <summary>
        /// Creates the always false condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateAlwaysFalseCondition(QueryContext context)
        {
            return new AlwaysFalseCondition();
        }

        /// <summary>
        /// Creates the and condition.
        /// </summary>
        /// <param name="context">The querz context.</param>
        /// <param name="conditions">The conditions.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateAndCondition(QueryContext context, IEnumerable<ICondition> conditions)
        {
            var and = new AndCondition();

            foreach (var cond in conditions)
            {
                and.AddToCondition(cond);
            }

            return and;
        }

        /// <summary>
        /// Creates the join equals condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="firstValBinder">The first value binder.</param>
        /// <param name="secondValBinder">The second value binder.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateJoinEqualsCondition(QueryContext context, IBaseValueBinder firstValBinder, IBaseValueBinder secondValBinder)
        {
            if (firstValBinder.VariableName != secondValBinder.VariableName)
                return new AlwaysTrueCondition();

            var orCondition = new OrCondition();
            orCondition.AddToCondition(CreateIsNullCondition(context, firstValBinder));
            orCondition.AddToCondition(CreateIsNullCondition(context, secondValBinder));
            orCondition.AddToCondition(CreateEqualsCondition(context, firstValBinder, secondValBinder));
            return orCondition;
        }

        /// <summary>
        /// Creates the is null condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="valueBinder">The value binder.</param>
        /// <returns>The created condition.</returns>
        /// <exception cref="System.Exception">Unknown value binder</exception>
        public ICondition CreateIsNullCondition(QueryContext context, IBaseValueBinder valueBinder)
        {
            if(valueBinder is ValueBinder)
            {
                var neededColumns = valueBinder.AssignedColumns.ToArray();

                if (neededColumns.Length == 0)
                    return CreateAlwaysFalseCondition(context);
                else if (neededColumns.Length == 1)
                    return CreateIsNullCondition(context, neededColumns[0]);
                else
                {
                    var orCondition = new OrCondition();
                    foreach (var column in neededColumns)
                    {
                        orCondition.AddToCondition(CreateIsNullCondition(context, column));
                    }
                    return orCondition;
                }
            }
            else if (valueBinder is CoalesceValueBinder)
            {
                var binders = ((CoalesceValueBinder)valueBinder).InnerBinders.ToArray();
                var andCondition = new AndCondition();

                foreach (var binder in binders)
                {
                    andCondition.AddToCondition(CreateIsNullCondition(context, binder));
                }

                return andCondition;
            }
            else if(valueBinder is CaseValueBinder)
            {
                var statements = ((CaseValueBinder)valueBinder).Statements.ToArray();
                var orCondition = new OrCondition();

                foreach (var statement in statements)
                {
                    var andCondition = new AndCondition();
                    andCondition.AddToCondition(statement.Condition);
                    andCondition.AddToCondition(CreateIsNullCondition(context, statement.ValueBinder));
                    orCondition.AddToCondition(andCondition);
                }

                return orCondition;
            }
            else if (valueBinder is ExpressionValueBinder)
            {
                var expression = ((ExpressionValueBinder)valueBinder).Expression;
                return CreateIsNullCondition(context, expression);
            }
            else if(valueBinder is BlankValueBinder)
            {
                return new AlwaysTrueCondition();
            }
            else
            {
                throw new Exception("Unknown value binder");
            }
        }

        /// <summary>
        /// Creates the is null condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>The created condition.</returns>
        /// <exception cref="System.Exception">Not implemented expression</exception>
        private ICondition CreateIsNullCondition(QueryContext context, IExpression expression)
        {
            if (expression is ColumnExpr)
            {
                var column = ((ColumnExpr)expression).Column;
                return CreateIsNullCondition(context, column);
            }
            else if (expression is CoalesceExpr)
            {
                var coalesce = (CoalesceExpr)expression;
                var conditions = coalesce.Expressions.Select(x => CreateIsNullCondition(context, x))
                    .ToArray();

                if (conditions.Length == 1)
                {
                    return conditions[0];
                }
                else
                {
                    var andCond = new AndCondition();

                    foreach (var cond in conditions)
                    {
                        andCond.AddToCondition(cond);
                    }

                    return andCond;
                }
            }
            else if (expression is CaseExpr)
            {
                var caseExpr = (CaseExpr)expression;
                List<ICondition> conditions = new List<ICondition>();

                foreach (var statement in caseExpr.Statements)
                {
                    var orCond = new OrCondition();
                    orCond.AddToCondition(new NotCondition(statement.Condition));
                    orCond.AddToCondition(CreateIsNullCondition(context, statement.Expression));
                    conditions.Add(orCond);
                }

                if(conditions.Count == 1)
                {
                    return conditions[0];
                }
                else
                {
                    var andCond = new AndCondition();

                    foreach (var cond in conditions)
                    {
                        andCond.AddToCondition(cond);
                    }

                    return andCond;
                }
            }
            else if (expression is ConcatenationExpr)
            {
                var concatExpr = (ConcatenationExpr)expression;
                var conditions = concatExpr.Parts.Select(x => CreateIsNullCondition(context, x))
                    .ToArray();

                if(conditions.Length == 1)
                {
                    return conditions[0];
                }
                else
                {
                    var orCond = new OrCondition();

                    foreach (var cond in conditions)
                    {
                        orCond.AddToCondition(cond);
                    }

                    return orCond;
                }
            }
            else if (expression is ConstantExpr)
            {
                return new AlwaysFalseCondition();
            }
            else if (expression is NullExpr)
            {
                return new AlwaysTrueCondition();
            }
            else
                throw new Exception("Not implemented expression");
        }

        /// <summary>
        /// Creates the is null condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="sqlColumn">The SQL column.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateIsNullCondition(QueryContext context, ISqlColumn sqlColumn)
        {
            return new IsNullCondition(sqlColumn);
        }

        /// <summary>
        /// Creates the is not null condition.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="sqlColumn">The SQL column.</param>
        /// <returns>The created condition.</returns>
        public ICondition CreateIsNotNullCondition(QueryContext context, ISqlColumn sqlColumn)
        {
            return new NotCondition(CreateIsNullCondition(context, sqlColumn));
        }
    }
}
