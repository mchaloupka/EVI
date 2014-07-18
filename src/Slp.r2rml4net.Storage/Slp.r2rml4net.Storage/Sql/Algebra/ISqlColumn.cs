using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    /// <summary>
    /// SQL Column
    /// </summary>
    public interface ISqlColumn
    {
        string Name { get; set; }

        ISqlSource Source { get; }
    }

    /// <summary>
    /// SQL Column that is really in the database
    /// </summary>
    public interface IOriginalSqlColumn : ISqlColumn
    {
        string OriginalName { get; }
    }
}
