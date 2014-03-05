using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class ConcatenationExpr : IExpression
    {
        private List<IExpression> parts;

        public ConcatenationExpr(IEnumerable<IExpression> parts)
        {
            this.parts = parts.ToList();
        }

        public IEnumerable<IExpression> Parts { get { return parts; } }

        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            var newParts = this.parts.Select(x => (IExpression)x.Clone());

            return new ConcatenationExpr(newParts);
        }
    }
}
