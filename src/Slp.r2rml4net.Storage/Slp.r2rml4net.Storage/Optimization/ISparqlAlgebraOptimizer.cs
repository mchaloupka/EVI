using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sparql;

namespace Slp.r2rml4net.Storage.Optimization
{
    public interface ISparqlAlgebraOptimizer
    {
        ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context);
    }
}
