using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public class IsNullCondition : ICondition
    {
        public ISqlColumn Column { get; set; }

        public IsNullCondition(ISqlColumn sqlColumn)
        {
            this.Column = sqlColumn;
        }

        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            return new IsNullCondition(this.Column);
        }
    }
}
