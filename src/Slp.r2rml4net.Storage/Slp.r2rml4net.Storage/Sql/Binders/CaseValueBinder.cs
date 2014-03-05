using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public class CaseValueBinder : IBaseValueBinder
    {
        private List<CaseStatementBinder> binders;

        public CaseValueBinder(string variableName)
        {
            binders = new List<CaseStatementBinder>();
            this.VariableName = variableName;
        }

        public void AddValueBinder(ICondition condition, IBaseValueBinder valueBinder)
        {
            if (valueBinder.VariableName != this.VariableName)
                throw new Exception("Cannot add value binder to case value binder with different variable name");

            binders.Add(new CaseStatementBinder(condition, valueBinder));
        }

        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            throw new NotImplementedException();
        }

        public string VariableName { get; private set; }

        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get
            {
                return this.binders.SelectMany(x => x.ValueBinder.AssignedColumns.Union(x.Condition.GetAllReferencedColumns())).Distinct();
            }
        }

        public IEnumerable<CaseStatementBinder> Statements { get { return binders; } }

        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            foreach (var binder in binders)
            {
                binder.ValueBinder.ReplaceAssignedColumn(oldColumn, newColumn);
                binder.Condition.ReplaceColumnReference(oldColumn, newColumn);
            }
        }

        public object Clone()
        {
            var newBinder = new CaseValueBinder(this.VariableName);

            foreach (var binder in this.binders)
            {
                newBinder.binders.Add(new CaseStatementBinder((ICondition)binder.Condition.Clone(), (IBaseValueBinder)binder.ValueBinder.Clone()));
            }

            return newBinder;
        }
    }

    public class CaseStatementBinder
    {
        public CaseStatementBinder(ICondition condition, IBaseValueBinder valueBinder)
        {
            this.Condition = condition;
            this.ValueBinder = valueBinder;
        }

        public ICondition Condition { get; private set; }

        public IBaseValueBinder ValueBinder { get; private set; }
    }
}
