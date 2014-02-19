using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql
{
    public interface ISparqlQuery
    {

    }

    public interface ISparqlQueryPart : ISparqlQuery
    {

    }

    public interface ISparqlQueryModifier : ISparqlQuery
    {
        ISparqlQuery InnerQuery { get; }
    }
}
