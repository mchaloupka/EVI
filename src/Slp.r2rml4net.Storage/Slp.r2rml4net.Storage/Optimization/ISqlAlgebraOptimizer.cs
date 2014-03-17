using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;

namespace Slp.r2rml4net.Storage.Optimization
{
    public interface ISqlAlgebraOptimizer
    {
        INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context);
    }

    public interface ISqlAlgebraOptimizerOnTheFly
    {
        INotSqlOriginalDbSource ProcessAlgebraOnTheFly(INotSqlOriginalDbSource algebra, QueryContext context);
    }
}
