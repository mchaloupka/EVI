using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class ConditionOptimizer : ISqlAlgebraOptimizer
    {
        public void ProcessAlgebra(ISqlQuery algebra, QueryContext context)
        {
            ProcessSource(algebra.SqlSource, context);
        }

        public void ProcessSource(ISqlSource source, QueryContext context)
        {
            if (source is SqlSelectOp)
            {
                ProcessSelectOp((SqlSelectOp)source, context);
            }
        }

        private void ProcessSelectOp(SqlSelectOp sqlSelectOp, QueryContext context)
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

        private void ProcessSelectOpJoinCondition(ConditionedSource join, QueryContext context)
        {
            var simplified = SimplifyCondition(join.Condition, context);
            if (join.Condition != simplified)
                join.ReplaceCondition(simplified);
        }

        private void ProcessSelectOpConditions(SqlSelectOp sqlSelectOp, QueryContext context)
        {
            var conditions = sqlSelectOp.Conditions.ToArray();

            foreach (var cond in conditions)
            {
                var simplified = SimplifyCondition(cond, context);

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

        private ICondition SimplifyCondition(ICondition condition, QueryContext context)
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
                return SimplifyAndCondition((AndCondition)condition, context);
            }
            else if (condition is OrCondition)
            {
                return SimplifyOrCondition((OrCondition)condition, context);
            }
            else if (condition is EqualsCondition)
            {
                return SimplifyEqualsCondition((EqualsCondition)condition, context);
            }

            throw new NotImplementedException();
        }

        private ICondition SimplifyOrCondition(OrCondition orCondition, QueryContext context)
        {
            throw new NotImplementedException();
        }

        private ICondition SimplifyAndCondition(AndCondition andCondition, QueryContext context)
        {
            throw new NotImplementedException();
        }

        private ICondition SimplifyEqualsCondition(EqualsCondition equalsCondition, QueryContext context)
        {
            var leftOp = equalsCondition.LeftOperand;
            var rightOp = equalsCondition.RightOperand;

            // TODO: Simplify concatenation

            if (ExpressionsAlwaysEqual(leftOp, rightOp))
            {
                return new AlwaysTrueCondition();
            }
            else if (ExpressionsCanBeEqual(leftOp, rightOp))
            {
                return new AlwaysFalseCondition();
            }
            else
            {
                return equalsCondition;
            }
        }

        private bool ExpressionsAlwaysEqual(IExpression first, IExpression second)
        {
            if (first is ConcatenationExpr || second is ConcatenationExpr) 
            {
                return false;
            }
            else if (first is ColumnExpr || second is ColumnExpr)
            {
                return false;
            }
            else if (first is ConstantExpr && second is ConstantExpr)
            {
                return ConstantExprAreEqual((ConstantExpr)first, (ConstantExpr)second);
            }

            throw new NotImplementedException();
        }

        private bool ExpressionsCanBeEqual(IExpression first, IExpression second)
        {
            if (first is ConcatenationExpr || second is ConcatenationExpr)
            {
                return true;
            }
            else if (first is ColumnExpr || second is ColumnExpr)
            {
                return true;
            }
            else if (first is ConstantExpr && second is ConstantExpr)
            {
                return ConstantExprAreEqual((ConstantExpr)first, (ConstantExpr)second);
            }

            throw new NotImplementedException();
        }

        private bool ConstantExprAreEqual(ConstantExpr constantExpr1, ConstantExpr constantExpr2)
        {
            return constantExpr1.SqlString == constantExpr2.SqlString;
        }
    }
}
