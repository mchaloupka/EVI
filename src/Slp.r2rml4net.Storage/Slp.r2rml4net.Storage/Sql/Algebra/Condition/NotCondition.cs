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

        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            return new NotCondition((ICondition)this.InnerCondition.Clone());
        }
    }
}
