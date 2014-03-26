using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    public static class StaticExpressionEvaluationExtension
    {
        private static readonly StaticExpressionEvaluationVisitor visitor = new StaticExpressionEvaluationVisitor();

        public static object StaticEvaluation(this IExpression expression, IQueryResultRow data)
        {
            return expression.Accept(visitor, data);
        }

        public static bool StaticEvaluation(this ICondition condition, IQueryResultRow data)
        {
            return (bool)condition.Accept(visitor, data);
        }

        private class StaticExpressionEvaluationVisitor : IExpressionVisitor, IConditionVisitor
        {
            public object Visit(ColumnExpr expression, object data)
            {
                var rData = (IQueryResultRow)data;
                var col = rData.GetColumn(expression.Column.Name);

                // TODO: Iri escape
                return col.Value;
            }

            public object Visit(ConstantExpr expression, object data)
            {
                return expression.Value;
            }

            public object Visit(ConcatenationExpr expression, object data)
            {
                var parts = expression.Parts.Select(x => x.Accept(this, data).ToString());
                return string.Join(string.Empty, parts);
            }

            public object Visit(NullExpr nullExpr, object data)
            {
                return null;
            }

            public object Visit(AlwaysFalseCondition condition, object data)
            {
                return false;
            }

            public object Visit(AlwaysTrueCondition condition, object data)
            {
                return true;
            }

            public object Visit(AndCondition condition, object data)
            {
                foreach (var inner in condition.Conditions)
                {
                    if (!(bool)inner.Accept(this, data))
                        return false;
                }

                return true;
            }

            public object Visit(EqualsCondition condition, object data)
            {
                var leftExpr = condition.LeftOperand.Accept(this, data);
                var rightExpr = condition.RightOperand.Accept(this, data);

                return leftExpr.Equals(rightExpr);
            }

            public object Visit(IsNullCondition condition, object data)
            {
                var rData = (IQueryResultRow)data;
                var col = rData.GetColumn(condition.Column.Name);

                return col.Value == null;
            }

            public object Visit(NotCondition condition, object data)
            {
                return !(bool)condition.InnerCondition.Accept(this, data);
            }

            public object Visit(OrCondition condition, object data)
            {
                foreach (var inner in condition.Conditions)
                {
                    if ((bool)inner.Accept(this, data))
                        return true;
                }

                return false;
            }

            public object Visit(CoalesceExpr collateExpr, object data)
            {
                object res = null;

                foreach (var expr in collateExpr.Expressions)
                {
                    res = expr.Accept(this, data);

                    if (res != null)
                        return res;
                }

                return null;
            }

            public object Visit(CaseExpr caseExpr, object data)
            {
                foreach (var statement in caseExpr.Statements)
                {
                    if ((bool)statement.Condition.Accept(this, data))
                        return statement.Expression.Accept(this, data);
                }

                return null;
            }
        }
    }
}
