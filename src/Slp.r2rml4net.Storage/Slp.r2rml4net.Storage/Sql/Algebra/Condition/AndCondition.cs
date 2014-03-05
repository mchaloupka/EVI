using System;
using System.Collections.Generic;
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

        public K Accept<K>(IConditionVisitor visitor)
        {
            return visitor.Visit<K>(this);
        }
    }
}
