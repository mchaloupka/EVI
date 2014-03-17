using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public class AndCondition : ICondition
    {
        private List<ICondition> conditions;
        public AndCondition()
        {
            this.conditions = new List<ICondition>();
        }

        public void AddToCondition(ICondition cond)
        {
            this.conditions.Add(cond);
        }

        public IEnumerable<ICondition> Conditions { get { return conditions; } }

        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            var newAnd = new AndCondition();

            foreach (var cond in this.conditions)
            {
                newAnd.conditions.Add((ICondition)cond.Clone());
            }

            return newAnd;
        }
    }
}
