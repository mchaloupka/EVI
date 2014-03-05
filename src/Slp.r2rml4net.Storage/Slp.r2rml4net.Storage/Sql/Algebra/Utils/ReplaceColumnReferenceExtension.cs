using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    public static class ReplaceColumnReferenceExtension
    {
        public static void ReplaceColumnReference(this ICondition condition, ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            var visitor = new ReplaceColumnReferenceVisitor(oldColumn, newColumn);
            condition.Accept(visitor, null);
        }

        public static void ReplaceColumnReference(this IExpression expression, ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            var visitor = new ReplaceColumnReferenceVisitor(oldColumn, newColumn);
            expression.Accept(visitor, null);
        }

        private class ReplaceColumnReferenceVisitor : IConditionVisitor, IExpressionVisitor
        {
            private ISqlColumn oldColumn;
            private ISqlColumn newColumn;

            public ReplaceColumnReferenceVisitor(ISqlColumn oldColumn, ISqlColumn newColumn)
            {
                this.oldColumn = oldColumn;
                this.newColumn = newColumn;
            }

            public object Visit(AlwaysFalseCondition condition, object data)
            {
                return null;
            }

            public object Visit(AlwaysTrueCondition condition, object data)
            {
                return null;
            }

            public object Visit(AndCondition condition, object data)
            {
                foreach (var item in condition.Conditions)
                {
                    item.Accept(this, data);
                }

                return null;
            }

            public object Visit(EqualsCondition condition, object data)
            {
                condition.LeftOperand.Accept(this, data);
                condition.RightOperand.Accept(this, data);
                return null;
            }

            public object Visit(IsNullCondition condition, object data)
            {
                if (condition.Column == this.oldColumn)
                    condition.Column = this.newColumn;

                return null;
            }

            public object Visit(NotCondition condition, object data)
            {
                condition.InnerCondition.Accept(this, data);
                return null;
            }

            public object Visit(OrCondition condition, object data)
            {
                foreach (var item in condition.Conditions)
                {
                    item.Accept(this, data);
                }

                return null;
            }

            public object Visit(ColumnExpr expression, object data)
            {
                if (expression.Column == oldColumn)
                    expression.Column = newColumn;

                return null;
            }

            public object Visit(ConstantExpr expression, object data)
            {
                return null;
            }

            public object Visit(ConcatenationExpr expression, object data)
            {
                foreach (var item in expression.Parts)
                {
                    item.Accept(this, data);
                }

                return null;
            }
        }
    }
}
