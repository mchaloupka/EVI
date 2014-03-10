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

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public abstract class BaseConditionOptimizer : ISqlAlgebraOptimizer, ISqlSourceVisitor, IConditionVisitor
    {
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            algebra.Accept(this, context);
            return algebra;
        }

        public object Visit(NoRowSource noRowSource, object data)
        {
            return null;
        }

        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return null;
        }

        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            sqlSelectOp.OriginalSource.Accept(this, data);

            foreach (var join in sqlSelectOp.JoinSources.Union(sqlSelectOp.LeftOuterJoinSources))
            {
                join.Source.Accept(this, data);

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

            return null;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            foreach (var source in sqlUnionOp.Sources)
            {
                source.Accept(this, data);
            }

            return null;
        }

        public object Visit(Sql.Algebra.Source.SqlStatement sqlStatement, object data)
        {
            return null;
        }

        public object Visit(Sql.Algebra.Source.SqlTable sqlTable, object data)
        {
            return null;
        }

        public object Visit(AlwaysFalseCondition condition, object data)
        {
            return ProcessAlwaysFalseCondition(condition, (QueryContext)data);
        }

        public object Visit(AlwaysTrueCondition condition, object data)
        {
            return ProcessAlwaysTrueCondition(condition, (QueryContext)data);
        }

        public object Visit(AndCondition condition, object data)
        {
            return ProcessAndCondition(condition, (QueryContext)data);
        }

        public object Visit(EqualsCondition condition, object data)
        {
            return ProcessEqualsCondition(condition, (QueryContext)data);
        }

        public object Visit(IsNullCondition condition, object data)
        {
            return ProcessIsNullCondition(condition, (QueryContext)data);
        }

        public object Visit(NotCondition condition, object data)
        {
            return ProcessNotCondition(condition, (QueryContext)data);
        }

        public object Visit(OrCondition condition, object data)
        {
            return ProcessOrCondition(condition, (QueryContext)data);
        }

        protected virtual ICondition ProcessAlwaysFalseCondition(AlwaysFalseCondition condition, QueryContext data)
        {
            return condition;
        }

        protected virtual ICondition ProcessAlwaysTrueCondition(AlwaysTrueCondition condition, QueryContext data)
        {
            return condition;
        }

        protected virtual ICondition ProcessIsNullCondition(IsNullCondition condition, QueryContext context)
        {
            return condition;
        }

        protected virtual ICondition ProcessNotCondition(NotCondition condition, QueryContext context)
        {
            var inner = ProcessCondition(condition.InnerCondition, context);

            if (inner is AlwaysTrueCondition)
                return new AlwaysFalseCondition();
            else if (inner is AlwaysFalseCondition)
                return new AlwaysTrueCondition();
            else
                return new NotCondition(inner);
        }

        protected ICondition ProcessCondition(ICondition condition, QueryContext context)
        {
            return (ICondition)condition.Accept(this, context);
        }

        protected virtual ICondition ProcessOrCondition(OrCondition orCondition, QueryContext context)
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in orCondition.Conditions)
            {
                var simpl = ProcessCondition(cond, context);

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

        protected virtual ICondition ProcessAndCondition(AndCondition andCondition, QueryContext context)
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in andCondition.Conditions)
            {
                var simpl = ProcessCondition(cond, context);

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

        protected virtual ICondition ProcessEqualsCondition(EqualsCondition equalsCondition, QueryContext context)
        {
            return equalsCondition;
        }
    }
}
