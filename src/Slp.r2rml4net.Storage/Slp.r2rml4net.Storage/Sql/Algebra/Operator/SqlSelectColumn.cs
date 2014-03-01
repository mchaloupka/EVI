using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlSelectColumn : ISqlColumn
    {
        public SqlSelectColumn(ISqlColumn originalColumn, ISqlSource source)
        {
            this.OriginalColumn = originalColumn;
            this.Source = source;
            this.Name = null;
        }

        public string Name { get; set; }

        public ISqlSource Source { get; private set; }

        public ISqlColumn OriginalColumn { get; private set; }
    }
}
