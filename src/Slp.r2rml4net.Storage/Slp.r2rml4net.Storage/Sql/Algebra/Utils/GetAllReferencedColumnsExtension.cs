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
        private static readonly GetAllReferencedColumnsVisitor visitor = new GetAllReferencedColumnsVisitor();

        public static IEnumerable<ISqlColumn> GetAllReferencedColumns(this ICondition condition)
        {
            return (IEnumerable<ISqlColumn>)condition.Accept(visitor, null);
        }

        public static IEnumerable<ISqlColumn> GetAllReferencedColumns(this IExpression expression)
        {
            return (IEnumerable<ISqlColumn>)expression.Accept(visitor, null);
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

            public IEnumerable<ISqlColumn> Visit(CoalesceExpr collateExpr, object data)
            {
                foreach (var subExpr in collateExpr.Expressions)
                {
                    var res = (IEnumerable<ISqlColumn>)subExpr.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            public IEnumerable<ISqlColumn> Visit(CaseExpr caseExpr, object data)
            {
                foreach (var subExpr in caseExpr.Statements)
                {
                    var res = (IEnumerable<ISqlColumn>)subExpr.Condition.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }

                    res = (IEnumerable<ISqlColumn>)subExpr.Expression.Accept(this, data);

                    foreach (var item in res)
                    {
                        yield return item;
                    }
                }
            }

            public IEnumerable<ISqlColumn> Visit(NullExpr nullExpr, object data)
            {
                yield break;
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

            object IExpressionVisitor.Visit(NullExpr expression, object data)
            {
                return Visit(expression, data);
            }

            object IExpressionVisitor.Visit(CoalesceExpr expression, object data)
            {
                return Visit(expression, data);
            }

            object IExpressionVisitor.Visit(CaseExpr expression, object data)
            {
                return Visit(expression, data);
            }
        }
    }
}
