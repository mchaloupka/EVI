using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class ColumnExpr : IExpression
    {
        public ColumnExpr(ISqlColumn column, bool isIriEscapedValue)
        {
            this.Column = column;
            this.IsIriEscapedValue = isIriEscapedValue;
        }

        public ISqlColumn Column { get; set; }

        public bool IsIriEscapedValue { get; set; }

        [DebuggerStepThrough]
        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            return new ColumnExpr(this.Column, this.IsIriEscapedValue);
        }
    }
}
