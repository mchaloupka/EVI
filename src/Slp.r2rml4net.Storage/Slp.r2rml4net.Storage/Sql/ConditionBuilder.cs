using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Binders;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql
{
    public class ConditionBuilder
    {
        private ExpressionBuilder expressionBuilder;

        public ConditionBuilder(ExpressionBuilder expressionBuilder)
        {
            this.expressionBuilder = expressionBuilder;
        }

        public ICondition CreateEqualsCondition(QueryContext context, INode node, IBaseValueBinder valueBinder)
        {
            if (valueBinder is ValueBinder)
            {
                var leftOperand = expressionBuilder.CreateExpression(context, (ValueBinder)valueBinder);
                var rightOperand = expressionBuilder.CreateExpression(context, node);

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

                var leftOperand = expressionBuilder.CreateColumnExpression(context, column, false);
                var rightOperand = expressionBuilder.CreateExpression(context, node);

                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if (valueBinder is ExpressionValueBinder)
            {
                var leftOperand = (IExpression)((ExpressionValueBinder)valueBinder).Expression.Clone();
                var rightOperand = expressionBuilder.CreateExpression(context, node);
                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if(valueBinder is BlankValueBinder)
            {
                return new AlwaysFalseCondition();
            }
            else
                throw new NotImplementedException("Not supported value binder in method CreateEqualsCondition");
        }

        public ICondition CreateEqualsCondition(QueryContext context, IBaseValueBinder firstValBinder, IBaseValueBinder secondValBinder)
        {
            if (firstValBinder is ValueBinder && secondValBinder is ValueBinder)
            {
                var leftOperand = expressionBuilder.CreateExpression(context, (ValueBinder)firstValBinder);
                var rightOperand = expressionBuilder.CreateExpression(context, (ValueBinder)secondValBinder);
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
                throw new Exception("Value binder can be only standard or collate");
        }

        public ICondition CreateEqualsCondition(QueryContext context, ISqlColumn column, IExpression expression)
        {
            return new EqualsCondition(expressionBuilder.CreateColumnExpression(context, column, false), expression);
        }

        public ICondition CreateEqualsCondition(QueryContext context, ISqlColumn childCol, ISqlColumn parentCol)
        {
            return new EqualsCondition(expressionBuilder.CreateColumnExpression(context, childCol, false), expressionBuilder.CreateColumnExpression(context, parentCol, false));
        }

        public ICondition CreateAlwaysTrueCondition(QueryContext context)
        {
            return new AlwaysTrueCondition();
        }

        public ICondition CreateAlwaysFalseCondition(QueryContext context)
        {
            return new AlwaysFalseCondition();
        }

        public ICondition CreateAndCondition(QueryContext context, IEnumerable<ICondition> conditions)
        {
            var and = new AndCondition();

            foreach (var cond in conditions)
            {
                and.AddToCondition(cond);
            }

            return and;
        }

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
                throw new Exception("Value binder can be only standard or collate");
            }
        }

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

        public ICondition CreateIsNullCondition(QueryContext context, ISqlColumn sqlColumn)
        {
            return new IsNullCondition(sqlColumn);
        }

        public ICondition CreateIsNotNullCondition(QueryContext context, ISqlColumn sqlColumn)
        {
            return new NotCondition(CreateIsNullCondition(context, sqlColumn));
        }
    }
}
