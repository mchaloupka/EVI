using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;

namespace Slp.r2rml4net.Storage.Optimization
{
    public interface ISqlAlgebraOptimizer
    {
        void ProcessAlgebra(ISqlQuery algebra, QueryContext context);
    }
}
