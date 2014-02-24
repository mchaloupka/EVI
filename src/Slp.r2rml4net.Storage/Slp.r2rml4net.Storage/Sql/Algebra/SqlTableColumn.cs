using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    public class SqlTableColumn : ISqlColumn
    {
        public SqlTableColumn(string name, ISqlSource source)
        {
            this.Name = name;
            this.Source = source;
        }

        public string Name { get; private set; }

        public ISqlSource Source { get; private set; }
    }
}
