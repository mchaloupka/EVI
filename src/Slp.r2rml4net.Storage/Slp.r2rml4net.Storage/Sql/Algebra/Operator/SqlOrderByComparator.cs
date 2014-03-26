using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlOrderByComparator
    {
        public IExpression Expression { get; set; }
        public bool Descending { get; private set; }

        public SqlOrderByComparator(IExpression expression, bool descending)
        {
            this.Expression = expression;
            this.Descending = descending;
        }

    }
}
