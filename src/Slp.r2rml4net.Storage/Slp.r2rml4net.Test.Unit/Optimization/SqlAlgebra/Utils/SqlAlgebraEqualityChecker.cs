using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Test.Unit.Optimization.SqlAlgebra.Utils
{
    public class SqlAlgebraEqualityChecker : IConditionVisitor, IExpressionVisitor
    {
        public object Visit(AlwaysFalseCondition condition, object data)
        {
            return data is AlwaysFalseCondition;
        }

        public object Visit(AlwaysTrueCondition condition, object data)
        {
            return data is AlwaysTrueCondition;
        }

        public object Visit(AndCondition condition, object data)
        {
            if(!(data is AndCondition))
            {
                return false;
            }

            var actual = (AndCondition)data;

            List<ICondition> leftConditions = condition.Conditions.ToList();
            List<ICondition> rightConditions = actual.Conditions.ToList();

            if (leftConditions.Count != rightConditions.Count)
                return false;

            int matched = 0;

            for (int l = 0; l < leftConditions.Count; l++)
            {
                var lCond = leftConditions[l];

                for(int r = 0; r < rightConditions.Count; r++)
                {
                    var rCond = rightConditions[r];

                    if((bool)lCond.Accept(this, rCond))
                    {
                        matched++;
                        break;
                    }
                }
            }

            if(matched != leftConditions.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object Visit(OrCondition condition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(EqualsCondition condition, object data)
        {
            if (!(data is EqualsCondition))
                return false;

            var actual = (EqualsCondition) data;

            var left1 = condition.LeftOperand;
            var right1 = condition.RightOperand;

            var left2 = actual.LeftOperand;
            var right2 = actual.RightOperand;

            return ((bool) left1.Accept(this, left2) && (bool) right1.Accept(this, right2)) ||
                   ((bool) left1.Accept(this, right2) && (bool) right1.Accept(this, left2));
        }

        public object Visit(IsNullCondition condition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(NotCondition condition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(ColumnExpr expression, object data)
        {
            if (!(data is ColumnExpr))
                return false;

            var actual = (ColumnExpr) data;

            return expression.Column == actual.Column && expression.IsIriEscapedValue == actual.IsIriEscapedValue;
        }

        public object Visit(ConstantExpr expression, object data)
        {
            if (!(data is ConstantExpr))
                return false;

            var actual = (ConstantExpr) data;

            return expression.Value.Equals(actual.Value);
        }

        public object Visit(ConcatenationExpr expression, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(NullExpr nullExpr, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(CoalesceExpr collateExpr, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(CaseExpr caseExpr, object data)
        {
            throw new NotImplementedException();
        }
    }
}
