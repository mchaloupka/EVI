using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class ConstantExpr : IExpression
    {
        // TODO: Value escaping
        // TODO: Connect with current db vendor

        public string SqlString { get; private set; }

        public object Value { get; private set; }

        public ConstantExpr(Uri uri)
        {
            SqlString = string.Format("\'{0}\'", uri.AbsoluteUri);
            Value = uri;
        }

        public ConstantExpr(string text)
        {
            SqlString = string.Format("\'{0}\'", text);
            Value = text;
        }

        public ConstantExpr(int number)
        {
            Value = number;
            SqlString = number.ToString();
        }

        [DebuggerStepThrough]
        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            if (Value is Uri)
            {
                return new ConstantExpr((Uri)Value);
            }
            else if (Value is int)
            {
                return new ConstantExpr((int)Value);
            }
            else if (Value is string)
            {
                return new ConstantExpr((string)Value);
            }
            else
                throw new NotImplementedException();
        }
    }
}
