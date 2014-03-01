using System;
using System.Collections.Generic;
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
    }
}
