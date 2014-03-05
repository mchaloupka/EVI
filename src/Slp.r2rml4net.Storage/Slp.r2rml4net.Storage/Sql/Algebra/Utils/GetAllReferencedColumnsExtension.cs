using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    public static class GetAllReferencedColumnsExtension
    {
        public static IEnumerable<ISqlColumn> GetAllReferencedColumns(this ICondition condition)
        {
            var visitor = new GetAllReferencedColumnsVisitor();

            var res = (IEnumerable<ISqlColumn>)condition.Accept(visitor, null);

            return res;
        }

        public static IEnumerable<ISqlColumn> GetAllReferencedColumns(this IExpression expression)
        {
            var visitor = new GetAllReferencedColumnsVisitor();

            var res = (IEnumerable<ISqlColumn>)expression.Accept(visitor, null);

            return res;
        }

        private class GetAllReferencedColumnsVisitor : IConditionVisitor, IExpressionVisitor
        {
            public IEnumerable<ISqlColumn> Visit(AlwaysFalseCondition condition, object data)
            {
                yield break;
            }

            public IEnumerable<ISqlColumn> Visit(AlwaysTrueCondition condition, object data)
            {
                yield break;
            }

            public IEnumerable<ISqlColumn> Visit(AndCondition condition, object data)
            {
                foreach (var subCond in condition.Conditions)
                {
                    var res = (IEnumerable<ISqlColumn>)subCond.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            public IEnumerable<ISqlColumn> Visit(EqualsCondition condition, object data)
            {
                var leftRes = (IEnumerable<ISqlColumn>)condition.LeftOperand.Accept(this, data);
                var rightRes = (IEnumerable<ISqlColumn>)condition.RightOperand.Accept(this, data);

                return leftRes.Union(rightRes);
            }

            public IEnumerable<ISqlColumn> Visit(IsNullCondition condition, object data)
            {
                yield return condition.Column;
            }

            public IEnumerable<ISqlColumn> Visit(NotCondition condition, object data)
            {
                return (IEnumerable<ISqlColumn>)condition.InnerCondition.Accept(this, data);
            }

            public IEnumerable<ISqlColumn> Visit(OrCondition condition, object data)
            {
                foreach (var subCond in condition.Conditions)
                {
                    var res = (IEnumerable<ISqlColumn>)subCond.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            public IEnumerable<ISqlColumn> Visit(ColumnExpr expression, object data)
            {
                yield return expression.Column;
            }

            public IEnumerable<ISqlColumn> Visit(ConstantExpr expression, object data)
            {
                yield break;
            }

            public IEnumerable<ISqlColumn> Visit(ConcatenationExpr expression, object data)
            {
                foreach (var subExpr in expression.Parts)
                {
                    var res = (IEnumerable<ISqlColumn>)subExpr.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            object IConditionVisitor.Visit(AlwaysFalseCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IConditionVisitor.Visit(AlwaysTrueCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IConditionVisitor.Visit(AndCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IConditionVisitor.Visit(EqualsCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IConditionVisitor.Visit(IsNullCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IConditionVisitor.Visit(OrCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IConditionVisitor.Visit(NotCondition condition, object data)
            {
                return Visit(condition, data);
            }

            object IExpressionVisitor.Visit(ColumnExpr expression, object data)
            {
                return Visit(expression, data);
            }

            object IExpressionVisitor.Visit(ConstantExpr expression, object data)
            {
                return Visit(expression, data);
            }

            object IExpressionVisitor.Visit(ConcatenationExpr expression, object data)
            {
                return Visit(expression, data);
            }
        }
    }
}
