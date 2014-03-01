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
    public abstract class BaseConditionOptimizer : ISqlAlgebraOptimizer
    {
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            ProcessSource(algebra, context);
            return algebra;
        }

        public void ProcessSource(ISqlSource source, QueryContext context)
        {
            if (source is SqlSelectOp)
            {
                ProcessSelectOp((SqlSelectOp)source, context);
            }
        }

        protected virtual void ProcessSelectOp(SqlSelectOp sqlSelectOp, QueryContext context)
        {
            ProcessSource(sqlSelectOp.OriginalSource, context);

            ProcessSelectOpConditions(sqlSelectOp, context);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                ProcessSelectOpJoinCondition(join, context);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                ProcessSelectOpJoinCondition(join, context);
            }
        }

        protected virtual void ProcessSelectOpJoinCondition(ConditionedSource join, QueryContext context)
        {
            var simplified = ProcessCondition(join.Condition, context);
            if (join.Condition != simplified)
                join.ReplaceCondition(simplified);
        }

        protected virtual void ProcessSelectOpConditions(SqlSelectOp sqlSelectOp, QueryContext context)
        {
            var conditions = sqlSelectOp.Conditions.ToArray();

            foreach (var cond in conditions)
            {
                var simplified = ProcessCondition(cond, context);

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
        }

        protected virtual ICondition ProcessCondition(ICondition condition, QueryContext context)
        {
            if (condition is AlwaysFalseCondition)
            {
                return condition;
            }
            else if (condition is AlwaysTrueCondition)
            {
                return condition;
            }
            else if (condition is AndCondition)
            {
                return ProcessAndCondition((AndCondition)condition, context);
            }
            else if (condition is OrCondition)
            {
                return ProcessOrCondition((OrCondition)condition, context);
            }
            else if (condition is EqualsCondition)
            {
                return ProcessEqualsCondition((EqualsCondition)condition, context);
            }

            throw new NotImplementedException();
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
