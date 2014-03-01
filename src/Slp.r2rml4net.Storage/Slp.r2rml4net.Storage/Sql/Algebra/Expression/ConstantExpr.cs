using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class ConstantExpr : IExpression
    {
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
    }
}
