using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class NoRowSource : INotSqlOriginalDbSource
    {
        public string Name { get; set; }


        public IEnumerable<ISqlColumn> Columns
        {
            get { yield break; }
        }

        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            throw new NotSupportedException("No row source cannot have any value binder");
        }

        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { yield break; }
        }

        public void ReplaceValueBinder(IBaseValueBinder oldBinder, IBaseValueBinder newBinder)
        {
            throw new NotSupportedException("No row source cannot have any value binder");
        }

        public void RemoveValueBinder(IBaseValueBinder valueBinder)
        {
            throw new NotSupportedException("No row source cannot have any value binder");
        }
    }
}
