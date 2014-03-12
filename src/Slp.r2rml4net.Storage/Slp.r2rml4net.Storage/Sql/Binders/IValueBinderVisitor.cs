using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public interface IValueBinderVisitor : IVisitor
    {
        object Visit(CaseValueBinder caseValueBinder, object data);

        object Visit(CollateValueBinder collateValueBinder, object data);

        object Visit(ValueBinder valueBinder, object data);
    }
}
