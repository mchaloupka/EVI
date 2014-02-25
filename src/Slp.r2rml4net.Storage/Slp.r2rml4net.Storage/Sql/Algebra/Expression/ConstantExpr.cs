using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class ConstantExpr : IExpression
    {
        private string sqlString;

        public ConstantExpr(Uri uri)
        {
            sqlString = string.Format("\'{0}\'", uri.AbsoluteUri);
        }

        public ConstantExpr(string text)
        {
            sqlString = string.Format("\'{0}\'", text);
        }

    }
}
