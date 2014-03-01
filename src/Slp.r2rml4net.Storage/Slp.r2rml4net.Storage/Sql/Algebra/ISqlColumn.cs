using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    public interface ISqlColumn
    {
        string Name { get; set; }

        ISqlSource Source { get; }
    }

    public interface IOriginalSqlColumn : ISqlColumn
    {
        string OriginalName { get; }
    }
}
