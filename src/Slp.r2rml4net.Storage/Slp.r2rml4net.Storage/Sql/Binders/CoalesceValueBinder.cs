using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public class CoalesceValueBinder : IBaseValueBinder
    {
        private List<IBaseValueBinder> binders;

        private CoalesceValueBinder()
        {
            this.binders = new List<IBaseValueBinder>();
        }

        public CoalesceValueBinder(IBaseValueBinder originalValueBinder)
        {
            this.binders = new List<IBaseValueBinder>();

            if (originalValueBinder is CoalesceValueBinder)
                this.binders.AddRange(((CoalesceValueBinder)originalValueBinder).binders);
            else
                this.binders.Add(originalValueBinder);
        }

        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            if (valueBinder.VariableName != VariableName)
                throw new Exception("Cannot collate value binders for different variables");

            if (valueBinder is CoalesceValueBinder)
                this.binders.AddRange(((CoalesceValueBinder)valueBinder).binders);
            else
                this.binders.Add(valueBinder);
        }

        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            foreach (var binder in this.binders)
            {
                var node = binder.LoadNode(factory, row, context);

                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        public string VariableName
        {
            get { return this.binders[0].VariableName; }
        }

        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { return this.binders.SelectMany(x => x.AssignedColumns).Distinct(); }
        }

        public IEnumerable<IBaseValueBinder> InnerBinders { get { return binders; } }


        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            foreach (var binder in binders)
            {
                binder.ReplaceAssignedColumn(oldColumn, newColumn);
            }
        }

        public object Clone()
        {
            var newBinder = new CoalesceValueBinder();

            foreach (var binder in this.InnerBinders)
            {
                newBinder.binders.Add((IBaseValueBinder)binder.Clone());
            }

            return newBinder;
        }

        [DebuggerStepThrough]
        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void ReplaceValueBinder(IBaseValueBinder binder, IBaseValueBinder newBinder)
        {
            var index = this.binders.IndexOf(binder);

            if (index > -1)
                this.binders[index] = newBinder;
        }

        public void RemoveValueBinder(IBaseValueBinder binder)
        {
            var index = this.binders.IndexOf(binder);

            if (index > -1)
                this.binders.RemoveAt(index);
        }
    }
}
