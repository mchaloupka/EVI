using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class NullExpr : IExpression
    {
        public object Clone()
        {
            return new NullExpr();
        }

        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
