using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlUnionOp : INotSqlOriginalDbSource
    {
        private List<SqlUnionColumn> columns;

        private List<SqlSelectOp> sources;

        private List<IBaseValueBinder> valueBinders;

        public SqlUnionOp()
        {
            this.CaseColumn = new SqlUnionColumn(this);
            this.columns = new List<SqlUnionColumn>();
            this.sources = new List<SqlSelectOp>();
            this.valueBinders = new List<IBaseValueBinder>();
        }

        public string Name { get; set; }

        public SqlUnionColumn CaseColumn { get; private set; }

        public IEnumerable<ISqlColumn> Columns
        {
            get
            {
                yield return this.CaseColumn;

                foreach (var col in this.columns)
                    yield return col;
            }
        }

        public void AddSource(SqlSelectOp select)
        {
            this.sources.Add(select);
        }

        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            this.valueBinders.Add(valueBinder);
        }

        public void ReplaceValueBinder(IBaseValueBinder fBinder, IBaseValueBinder sBinder)
        {
            var index = this.valueBinders.IndexOf(fBinder);

            if (index > -1)
                this.valueBinders[index] = sBinder;
        }

        public void RemoveValueBinder(IBaseValueBinder valueBinder)
        {
            var index = this.valueBinders.IndexOf(valueBinder);

            if (index > -1)
                this.valueBinders.RemoveAt(index);
        }

        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { return this.valueBinders; }
        }

        public SqlUnionColumn GetUnionedColumn()
        {
            var col = new SqlUnionColumn(this);
            this.columns.Add(col);
            return col;
        }

        public IEnumerable<SqlSelectOp> Sources { get { return sources; } }

        [DebuggerStepThrough]
        public object Accept(ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void RemoveColumn(ISqlColumn col)
        {
            if(col is SqlUnionColumn)
            {
                this.columns.Remove((SqlUnionColumn)col);
            }
        }

        public void RemoveSource(ISqlSource source)
        {
            if (source is SqlSelectOp)
            {
                this.sources.Remove((SqlSelectOp)source);

                List<SqlUnionColumn> colToDelete = new List<SqlUnionColumn>();

                foreach (var col in this.columns)
                {
                    var containedColumns = col.OriginalColumns.Where(x => x.Source == source).ToList();

                    foreach (var ccol in containedColumns)
                    {
                        col.RemoveColumn(ccol);
                    }
                }
                
                foreach (var valBinder in this.valueBinders.Cast<CaseValueBinder>())
                {
                    var sourceStatements = valBinder.Statements.Where(x => x.Condition.GetAllReferencedColumns().All(y => y.Source == source)).ToList();

                    foreach (var statement in sourceStatements)
                    {
                        valBinder.RemoveStatement(statement);
                    }
                }
            }
        }
    }
}
