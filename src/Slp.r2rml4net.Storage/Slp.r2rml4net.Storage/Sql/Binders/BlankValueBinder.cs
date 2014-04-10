using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public class BlankValueBinder : IBaseValueBinder
    {
        public BlankValueBinder(string variableName)
        {
            this.VariableName = variableName;
        }

        public VDS.RDF.INode LoadNode(VDS.RDF.INodeFactory factory, IQueryResultRow row, Query.QueryContext context)
        {
            return null;
        }

        public string VariableName { get; private set; }

        public IEnumerable<Algebra.ISqlColumn> AssignedColumns
        {
            get { yield break; }
        }

        public void ReplaceAssignedColumn(Algebra.ISqlColumn oldColumn, Algebra.ISqlColumn newColumn)
        {
            
        }

        public object Clone()
        {
            return new BlankValueBinder(this.VariableName);
        }

        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
