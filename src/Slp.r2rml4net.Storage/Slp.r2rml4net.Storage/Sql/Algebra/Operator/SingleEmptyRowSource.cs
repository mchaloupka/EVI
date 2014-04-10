using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SingleEmptyRowSource : INotSqlOriginalDbSource
    {
        private List<IBaseValueBinder> valueBinders;

        public SingleEmptyRowSource()
        {
            valueBinders = new List<IBaseValueBinder>();
        }

        public string Name { get; set; }

        public IEnumerable<ISqlColumn> Columns
        {
            get { yield break; }
        }

        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            this.valueBinders.Add(valueBinder);
        }

        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { return valueBinders; }
        }

        public void ReplaceValueBinder(IBaseValueBinder oldBinder, IBaseValueBinder newBinder)
        {
            var index = this.valueBinders.IndexOf(oldBinder);

            if (index > -1)
                this.valueBinders[index] = newBinder;
        }

        public void RemoveValueBinder(IBaseValueBinder valueBinder)
        {
            var index = this.valueBinders.IndexOf(valueBinder);

            if (index > -1)
                this.valueBinders.RemoveAt(index);
        }

        [DebuggerStepThrough]
        public object Accept(ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
