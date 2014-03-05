using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Utils
{
    public interface IVisitable<T> where T : IVisitor
    {
        object Accept(T visitor, object data);
    }
}
