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
    public class ExpressionValueBinder : IBaseValueBinder
    {
        public ExpressionValueBinder(string variableName, IExpression expression)
        {
            this.VariableName = variableName;
            this.Expression = expression;
        }

        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            // NOTE: I should work with types
            var value = this.Expression.StaticEvaluation(row);

            if (value != null)
                return factory.CreateLiteralNode(value.ToString());
            else
                return null;
        }

        public string VariableName { get; private set; }

        public IExpression Expression { get; set; }

        public IEnumerable<Algebra.ISqlColumn> AssignedColumns
        {
            get { return this.Expression.GetAllReferencedColumns(); }
        }

        public void ReplaceAssignedColumn(Algebra.ISqlColumn oldColumn, Algebra.ISqlColumn newColumn)
        {
            this.Expression.ReplaceColumnReference(oldColumn, newColumn);
        }

        public object Clone()
        {
            return new ExpressionValueBinder(this.VariableName, (IExpression)this.Expression.Clone());
        }

        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
