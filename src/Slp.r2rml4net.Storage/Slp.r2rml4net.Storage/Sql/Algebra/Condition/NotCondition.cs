using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public class NotCondition : ICondition
    {
        public ICondition InnerCondition { get; private set; }

        public NotCondition(ICondition condition)
        {
            this.InnerCondition = condition;
        }
    }
}
