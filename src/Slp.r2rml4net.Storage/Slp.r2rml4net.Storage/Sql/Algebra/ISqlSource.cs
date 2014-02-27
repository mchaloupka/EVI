using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    public interface ISqlSource
    {
        string Name { get; set; }

        IEnumerable<ISqlColumn> Columns { get; }
    }

    public interface ISqlOriginalDbSource : ISqlSource
    {
        ISqlColumn GetColumn(string column);
    }
}
