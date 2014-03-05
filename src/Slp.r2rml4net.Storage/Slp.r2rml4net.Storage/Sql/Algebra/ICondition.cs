using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    public interface ICondition : ICloneable, IVisitable<IConditionVisitor>
    {

    }
}
