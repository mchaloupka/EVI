using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public class AlwaysTrueCondition : ICondition
    {
        public K Accept<K>(IConditionVisitor visitor)
        {
            return visitor.Visit<K>(this);
        }
    }
}
