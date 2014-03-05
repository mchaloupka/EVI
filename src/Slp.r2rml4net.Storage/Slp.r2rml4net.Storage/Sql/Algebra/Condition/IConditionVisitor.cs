using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public interface IConditionVisitor : IVisitor
    {
        K Visit<K>(AlwaysFalseCondition condition);
        K Visit<K>(AlwaysTrueCondition condition);
        K Visit<K>(AndCondition condition);
        K Visit<K>(EqualsCondition condition);
        K Visit<K>(IsNullCondition condition);
        K Visit<K>(NotCondition condition);
        K Visit<K>(OrCondition condition);
    }
}
