using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Test.Unit.Relational.Utilities
{
    public class SqlAlgebraEqualityChecker 
        : IConditionVisitor, IExpressionVisitor
    {
        public object Visit(AlwaysFalseCondition condition, object data)
        {
            return data is AlwaysFalseCondition;
        }

        public object Visit(AlwaysTrueCondition condition, object data)
        {
            return data is AlwaysTrueCondition;
        }

        public object Visit(ConjunctionCondition condition, object data)
        {
            if (!(data is ConjunctionCondition))
            {
                return false;
            }

            var actual = (ConjunctionCondition)data;

            List<IFilterCondition> leftConditions = condition.InnerConditions.ToList();
            List<IFilterCondition> rightConditions = actual.InnerConditions.ToList();

            if (leftConditions.Count != rightConditions.Count)
                return false;

            int matched = 0;

            for (int l = 0; l < leftConditions.Count; l++)
            {
                var lCond = leftConditions[l];

                for (int r = 0; r < rightConditions.Count; r++)
                {
                    var rCond = rightConditions[r];

                    if ((bool)lCond.Accept(this, rCond))
                    {
                        matched++;
                        break;
                    }
                }
            }

            if (matched != leftConditions.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object Visit(DisjunctionCondition condition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(EqualExpressionCondition equalExpressionCondition, object data)
        {
            if (!(data is EqualExpressionCondition))
                return false;

            var actual = (EqualExpressionCondition)data;

            var left1 = equalExpressionCondition.LeftOperand;
            var right1 = equalExpressionCondition.RightOperand;

            var left2 = actual.LeftOperand;
            var right2 = actual.RightOperand;

            return ((bool)left1.Accept(this, left2) && (bool)right1.Accept(this, right2)) ||
                   ((bool)left1.Accept(this, right2) && (bool)right1.Accept(this, left2));
        }

        public object Visit(EqualVariablesCondition equalVariablesCondition, object data)
        {
            if (!(data is EqualVariablesCondition))
                return false;

            var actual = (EqualVariablesCondition)data;

            var left1 = equalVariablesCondition.LeftVariable;
            var right1 = equalVariablesCondition.RightVariable;

            var left2 = actual.LeftVariable;
            var right2 = actual.RightVariable;

            return (left1 == left2 && right1 == right2) ||
                   (left1 == right2 && right1 == left2);
        }

        public object Visit(IsNullCondition condition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(NegationCondition condition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(UnionedSourcesCondition unionedSourcesCondition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(ColumnExpression columnExpression, object data)
        {
            if (!(data is ColumnExpression))
                return false;

            var actual = (ColumnExpression)data;

            return columnExpression.CalculusVariable == actual.CalculusVariable && columnExpression.IsUri == actual.IsUri;
        }

        public object Visit(ConcatenationExpression concatenationExpression, object data)
        {
            if (!(data is ConcatenationExpression))
                return false;

            var actual = (ConcatenationExpression)data;

            var expectedInnerExpressions = concatenationExpression.InnerExpressions.ToArray();
            var actualInnerExpressions = actual.InnerExpressions.ToArray();

            if (actualInnerExpressions.Length != expectedInnerExpressions.Length)
                return false;

            for (int i = 0; i < expectedInnerExpressions.Length; i++)
            {
                if(!(bool)expectedInnerExpressions[i].Accept(this, actualInnerExpressions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public object Visit(ConstantExpression constantExpression, object data)
        {
            if (!(data is ConstantExpression))
                return false;

            var actual = (ConstantExpression)data;

            return constantExpression.Value.Equals(actual.Value);
        }

        public object Visit(AssignmentFromExpressionCondition assignmentFromExpressionCondition, object data)
        {
            throw new NotImplementedException();
        }
    }
}
