using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class ConditionedSource
    {
        public ICondition Condition { get; private set; }

        public ISqlSource Source { get; private set; }

        public ConditionedSource(ICondition condition, ISqlSource source)
        {
            this.Condition = condition;
            this.Source = source;
        }

        public void ReplaceCondition(ICondition newCondition)
        {
            this.Condition = newCondition;
        }
    }
}
