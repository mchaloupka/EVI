using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public interface IExpressionVisitor : IVisitor
    {
        object Visit(ColumnExpr expression, object data);
        object Visit(ConstantExpr expression, object data);
        object Visit(ConcatenationExpr expression, object data);
    }
}
