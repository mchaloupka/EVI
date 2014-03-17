using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<CaseStatementBinder> statements;

        public CaseValueBinder(string variableName)
        {
            statements = new List<CaseStatementBinder>();
            this.VariableName = variableName;
        }

        public void AddValueBinder(ICondition condition, IBaseValueBinder valueBinder)
        {
            if (valueBinder.VariableName != this.VariableName)
                throw new Exception("Cannot add value binder to case value binder with different variable name");

            statements.Add(new CaseStatementBinder(condition, valueBinder));
        }

        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            var matchingStatement = statements.Where(x => x.Condition.StaticEvaluation(row)).FirstOrDefault();

            if(matchingStatement != null)
            {
                return matchingStatement.ValueBinder.LoadNode(factory, row, context);
            }
            else
            {
                throw new Exception("Every row should be in some case");
            }
        }

        public string VariableName { get; private set; }

        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get
            {
                return this.statements.SelectMany(x => x.ValueBinder.AssignedColumns.Union(x.Condition.GetAllReferencedColumns())).Distinct();
            }
        }

        public IEnumerable<CaseStatementBinder> Statements { get { return statements; } }

        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            foreach (var binder in statements)
            {
                binder.ValueBinder.ReplaceAssignedColumn(oldColumn, newColumn);
                binder.Condition.ReplaceColumnReference(oldColumn, newColumn);
            }
        }

        public object Clone()
        {
            var newBinder = new CaseValueBinder(this.VariableName);

            foreach (var binder in this.statements)
            {
                newBinder.statements.Add(new CaseStatementBinder((ICondition)binder.Condition.Clone(), (IBaseValueBinder)binder.ValueBinder.Clone()));
            }

            return newBinder;
        }

        [DebuggerStepThrough]
        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void RemoveStatement(CaseStatementBinder caseStatement)
        {
            this.statements.Remove(caseStatement);
        }
    }

    public class CaseStatementBinder
    {
        public CaseStatementBinder(ICondition condition, IBaseValueBinder valueBinder)
        {
            this.Condition = condition;
            this.ValueBinder = valueBinder;
        }

        public ICondition Condition { get; set; }

        public IBaseValueBinder ValueBinder { get; set; }
    }
}
