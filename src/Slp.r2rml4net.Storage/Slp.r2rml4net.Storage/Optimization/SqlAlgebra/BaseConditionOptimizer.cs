using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public abstract class BaseConditionOptimizer : ISqlAlgebraOptimizer, ISqlAlgebraOptimizerOnTheFly, ISqlSourceVisitor, IConditionVisitor, IExpressionVisitor
    {
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, new VisitData() { Context = context, SecondRun = false, IsOnTheFly = false });
        }

        public INotSqlOriginalDbSource ProcessAlgebraOnTheFly(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, new VisitData() { Context = context, SecondRun = false, IsOnTheFly = true });
        }

        public object Visit(NoRowSource noRowSource, object data)
        {
            return noRowSource;
        }

        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return singleEmptyRowSource;
        }

        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            var vd = (VisitData)data;

            if (!vd.IsOnTheFly)
            {
                sqlSelectOp.OriginalSource.Accept(this, data);
            }

            foreach (var column in sqlSelectOp.Columns.OfType<SqlExpressionColumn>())
            {
                var newExpr = (IExpression)column.Expression.Accept(this, data);

                if (newExpr != column.Expression)
                    column.Expression = newExpr;
            }

            foreach (var ordering in sqlSelectOp.Orderings)
            {
                ordering.Expression = (IExpression)ordering.Expression.Accept(this, data);
            }

            foreach (var join in sqlSelectOp.JoinSources.Union(sqlSelectOp.LeftOuterJoinSources))
            {
                if (!vd.IsOnTheFly)
                {
                    join.Source.Accept(this, data);
                }

                var simplified = (ICondition)join.Condition.Accept(this, data);

                if (simplified != join.Condition)
                    join.ReplaceCondition(simplified);
            }

            var conditions = sqlSelectOp.Conditions.ToArray();

            foreach (var cond in conditions)
            {
                var simplified = (ICondition)cond.Accept(this, data);

                if (simplified != cond)
                    sqlSelectOp.ReplaceCondition(cond, simplified);
            }

            var alwaysTrue = sqlSelectOp.Conditions.OfType<AlwaysTrueCondition>().ToArray();

            foreach (var cond in alwaysTrue)
            {
                sqlSelectOp.RemoveCondition(cond);
            }

            if (sqlSelectOp.Conditions.OfType<AlwaysFalseCondition>().Any())
            {
                sqlSelectOp.ClearConditions();
                sqlSelectOp.AddCondition(new AlwaysFalseCondition());
            }

            if (vd.IsOnTheFly)
            {
                if ((sqlSelectOp.OriginalSource is NoRowSource)
                    || sqlSelectOp.JoinSources.Select(x => x.Source).OfType<NoRowSource>().Any()
                    || sqlSelectOp.JoinSources.Select(x => x.Condition).OfType<AlwaysFalseCondition>().Any()
                    || sqlSelectOp.Conditions.OfType<AlwaysFalseCondition>().Any())
                {
                    var noRowSource = new NoRowSource();

                    foreach (var valBinder in sqlSelectOp.ValueBinders)
                    {
                        noRowSource.AddValueBinder(new BlankValueBinder(valBinder.VariableName));
                    }

                    return noRowSource;
                }
            }

            return sqlSelectOp;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            var vd = (VisitData)data;

            if (!vd.IsOnTheFly)
            {
                foreach (var source in sqlUnionOp.Sources)
                {
                    source.Accept(this, data);
                }
            }

            return sqlUnionOp;
        }

        public object Visit(Sql.Algebra.Source.SqlStatement sqlStatement, object data)
        {
            return sqlStatement;
        }

        public object Visit(Sql.Algebra.Source.SqlTable sqlTable, object data)
        {
            return sqlTable;
        }

        public object Visit(AlwaysFalseCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessAlwaysFalseCondition(condition, visitData.Context);
            else
                return _ProcessAlwaysFalseCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        public object Visit(AlwaysTrueCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessAlwaysTrueCondition(condition, visitData.Context);
            else
                return _ProcessAlwaysTrueCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        public object Visit(AndCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessAndCondition(condition, visitData.Context);
            else
                return _ProcessAndCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        public object Visit(EqualsCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessEqualsCondition(condition, visitData.Context);
            else
                return _ProcessEqualsCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        public object Visit(IsNullCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessIsNullCondition(condition, visitData.Context);
            else
                return _ProcessIsNullCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        public object Visit(NotCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessNotCondition(condition, visitData.Context);
            else
                return _ProcessNotCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        public object Visit(OrCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessOrCondition(condition, visitData.Context);
            else
                return _ProcessOrCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        private ICondition _ProcessAlwaysFalseCondition(AlwaysFalseCondition condition, VisitData data)
        {
            return condition;
        }

        private ICondition _ProcessAlwaysTrueCondition(AlwaysTrueCondition condition, VisitData data)
        {
            return condition;
        }

        private ICondition _ProcessIsNullCondition(IsNullCondition condition, VisitData data)
        {
            return condition;
        }

        private ICondition _ProcessNotCondition(NotCondition condition, VisitData data)
        {
            var inner = (ICondition)condition.InnerCondition.Accept(this, data);

            if (inner is AlwaysTrueCondition)
                return new AlwaysFalseCondition();
            else if (inner is AlwaysFalseCondition)
                return new AlwaysTrueCondition();
            else
                return new NotCondition(inner);
        }

        private ICondition _ProcessOrCondition(OrCondition orCondition, VisitData data)
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in orCondition.Conditions)
            {
                var simpl = (ICondition)cond.Accept(this, data);

                if (simpl is AlwaysTrueCondition)
                    return new AlwaysTrueCondition();
                else if (!(simpl is AlwaysFalseCondition))
                    conditions.Add(simpl);
            }

            if (conditions.Count == 0)
                return new AlwaysFalseCondition();
            else if (conditions.Count == 1)
                return conditions[0];
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

        private ICondition _ProcessAndCondition(AndCondition andCondition, VisitData data)
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in andCondition.Conditions)
            {
                var simpl = (ICondition)cond.Accept(this, data);

                if (simpl is AlwaysFalseCondition)
                    return new AlwaysFalseCondition();
                else if (!(simpl is AlwaysTrueCondition))
                    conditions.Add(simpl);
            }

            if (conditions.Count == 0)
                return new AlwaysTrueCondition();
            else if (conditions.Count == 1)
                return conditions[0];
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

        private ICondition _ProcessEqualsCondition(EqualsCondition equalsCondition, VisitData data)
        {
            var leftOperand = (IExpression)equalsCondition.LeftOperand.Accept(this, data);
            var rightOperand = (IExpression)equalsCondition.RightOperand.Accept(this, data);

            equalsCondition.LeftOperand = leftOperand;
            equalsCondition.RightOperand = rightOperand;

            return equalsCondition;
        }

        public object Visit(ColumnExpr expression, object data)
        {
            return expression;
        }

        public object Visit(ConstantExpr expression, object data)
        {
            return expression;
        }

        public object Visit(ConcatenationExpr expression, object data)
        {
            foreach (var part in expression.Parts.ToArray())
            {
                var newPart = (IExpression)part.Accept(this, data);

                if (newPart != part)
                    expression.ReplacePart(part, newPart);
            }

            return expression;
        }

        public object Visit(NullExpr nullExpr, object data)
        {
            return nullExpr;
        }

        public object Visit(CoalesceExpr collateExpr, object data)
        {
            foreach (var subExpr in collateExpr.Expressions.ToArray())
            {
                var newExpr = (IExpression)subExpr.Accept(this, data);

                if (newExpr is NullExpr)
                {
                    collateExpr.RemoveExpression(subExpr);
                }
                else if (newExpr != subExpr)
                    collateExpr.ReplaceExpression(subExpr, newExpr);
            }

            if(collateExpr.Expressions.Any())
            {
                if(collateExpr.Expressions.Count() == 1)
                {
                    return collateExpr.Expressions.First();
                }
                else
                {
                    return collateExpr;
                }
            }
            else
            {
                return new NullExpr();
            }
        }

        public object Visit(CaseExpr caseExpr, object data)
        {
            bool removeRest = false;

            foreach (var statement in caseExpr.Statements.ToArray())
            {
                if (removeRest)
                {
                    caseExpr.RemoveStatement(statement);
                }
                else
                {
                    var newCond = (ICondition)statement.Condition.Accept(this, data);

                    if (newCond is AlwaysFalseCondition)
                    {
                        caseExpr.RemoveStatement(statement);
                    }
                    else
                    {
                        var newExpr = (IExpression)statement.Expression.Accept(this, data);

                        statement.Condition = newCond;
                        statement.Expression = newExpr;

                        if (statement.Condition is AlwaysTrueCondition)
                        {
                            removeRest = true;
                        }
                    }
                }
            }

            if (caseExpr.Statements.Any())
                return caseExpr;
            else
                return new NullExpr();
        }

        protected virtual ICondition ProcessAlwaysFalseCondition(AlwaysFalseCondition condition, QueryContext visitData)
        {
            return condition;
        }

        protected virtual ICondition ProcessAlwaysTrueCondition(AlwaysTrueCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        protected virtual ICondition ProcessAndCondition(AndCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        protected virtual ICondition ProcessEqualsCondition(EqualsCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        protected virtual ICondition ProcessIsNullCondition(IsNullCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        protected virtual ICondition ProcessNotCondition(NotCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        protected virtual ICondition ProcessOrCondition(OrCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        private class VisitData
        {
            public QueryContext Context { get; set; }
            public bool SecondRun { get; set; }
            public bool IsOnTheFly { get; set; }

            public VisitData ForSecondRun()
            {
                return new VisitData()
                {
                    Context = Context,
                    SecondRun = true,
                    IsOnTheFly = IsOnTheFly
                };
            }
        }
    }
}
