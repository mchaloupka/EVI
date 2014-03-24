using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlUnionColumn : ISqlColumn
    {
        private List<ISqlColumn> originalColumns;

        public SqlUnionColumn(ISqlSource source)
        {
            this.Source = source;
            this.originalColumns = new List<ISqlColumn>();
        }

        public void AddColumn(ISqlColumn column)
        {
            this.originalColumns.Add(column);
        }

        public IEnumerable<ISqlColumn> OriginalColumns { get { return this.originalColumns; } }

        public string Name { get; set; }

        public ISqlSource Source { get; private set; }

        public void RemoveColumn(ISqlColumn ccol)
        {
            this.originalColumns.Remove(ccol);
        }
    }
}
