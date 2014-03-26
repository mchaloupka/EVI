using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlExpressionColumn : ISqlColumn
    {
        public SqlExpressionColumn(IExpression expression, ISqlSource source)
        {
            this.Expression = expression;
            this.Source = source;
        }

        public string Name { get; set; }

        public ISqlSource Source { get; private set; }

        public IExpression Expression { get; set; }
    }
}
