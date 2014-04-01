using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public class SqlSideValueBinder : IBaseValueBinder
    {
        public ISqlColumn Column { get; private set; }

        public SqlSideValueBinder(ISqlColumn column, IBaseValueBinder originalBinder)
        {
            this.OriginalBinder = originalBinder;
            this.Column = column;
        }

        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            var value = row.GetColumn(this.Column.Name).Value;

            if (value == null)
                return null;

            return factory.CreateLiteralNode(value.ToString());
        }

        public string VariableName { get { return OriginalBinder.VariableName; } }

        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { yield return this.Column; }
        }

        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            if (oldColumn == Column)
                Column = newColumn;
        }

        public object Clone()
        {
            return new SqlSideValueBinder(Column, OriginalBinder);
        }

        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public IBaseValueBinder OriginalBinder { get; private set; }
    }
}
