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
        object Visit(AlwaysFalseCondition condition, object data);
        object Visit(AlwaysTrueCondition condition, object data);
        object Visit(AndCondition condition, object data);
        object Visit(OrCondition condition, object data);
        object Visit(EqualsCondition condition, object data);
        object Visit(IsNullCondition condition, object data);
        object Visit(NotCondition condition, object data);
    }
}
