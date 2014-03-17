using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public class OrCondition : ICondition
    {
        private List<ICondition> conditions;

        public OrCondition()
        {
            this.conditions = new List<ICondition>();
        }

        public void AddToCondition(ICondition condition)
        {
            conditions.Add(condition);
        }

        public IEnumerable<ICondition> Conditions { get { return conditions; } }

        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            var orCondition = new OrCondition();

            foreach (var cond in this.conditions)
            {
                orCondition.conditions.Add((ICondition)cond.Clone());
            }

            return orCondition;
        }
    }
}
