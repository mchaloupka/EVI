using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class CoalesceExpr : IExpression
    {
        private List<IExpression> expressions;

        public CoalesceExpr()
        {
            this.expressions = new List<IExpression>();
        }

        public void AddExpression(IExpression expression)
        {
            this.expressions.Add(expression);
        }

        public IEnumerable<IExpression> Expressions { get { return expressions; } }

        public object Clone()
        {
            var col = new CoalesceExpr();
            foreach (var expr in expressions)
            {
                col.expressions.Add((IExpression)expr.Clone());
            }
            return col;
        }

        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void ReplaceExpression(IExpression oldExpr, IExpression newExpr)
        {
            var index = expressions.IndexOf(oldExpr);

            if (index > -1)
                expressions[index] = newExpr;
        }

        public void RemoveExpression(IExpression subExpr)
        {
            var index = expressions.IndexOf(subExpr);

            if (index > -1)
                expressions.RemoveAt(index);
        }
    }
}
