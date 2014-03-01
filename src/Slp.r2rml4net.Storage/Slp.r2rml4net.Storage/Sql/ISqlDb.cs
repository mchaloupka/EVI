using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;

namespace Slp.r2rml4net.Storage.Sql
{
    public interface ISqlDb
    {
        string GenerateQuery(INotSqlOriginalDbSource sqlAlgebra, QueryContext context);

        IQueryResultReader ExecuteQuery(string query, QueryContext context);
    }
}
