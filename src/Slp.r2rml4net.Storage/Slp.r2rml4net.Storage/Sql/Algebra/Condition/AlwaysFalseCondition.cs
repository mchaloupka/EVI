using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public class AlwaysFalseCondition : ICondition
    {
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            return new AlwaysFalseCondition();
        }
    }
}
