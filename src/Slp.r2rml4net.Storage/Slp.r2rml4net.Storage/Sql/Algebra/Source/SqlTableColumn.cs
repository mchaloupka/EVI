using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    public class SqlTableColumn : IOriginalSqlColumn
    {
        public SqlTableColumn(string name, ISqlSource source)
        {
            this.OriginalName = name;
            this.Source = source;
        }

        public string OriginalName { get; private set; }

        public string Name { get; set; }

        public ISqlSource Source { get; private set; }
    }
}
