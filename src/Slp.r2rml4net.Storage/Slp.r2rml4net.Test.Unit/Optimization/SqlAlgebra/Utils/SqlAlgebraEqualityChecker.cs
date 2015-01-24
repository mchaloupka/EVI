using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Test.Unit.Optimization.SqlAlgebra.Utils
{
    public class SqlAlgebraEqualityChecker : IConditionVisitor
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
            throw new NotImplementedException();
        }

        public object Visit(IsNullCondition condition, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(NotCondition condition, object data)
        {
            throw new NotImplementedException();
        }
    }
}
