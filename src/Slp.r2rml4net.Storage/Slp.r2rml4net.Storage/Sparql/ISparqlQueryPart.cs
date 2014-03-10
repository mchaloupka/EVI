using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sparql
{
    public interface ISparqlQuery : IVisitable<ISparqlQueryVisitor>
    {
        IEnumerable<ISparqlQuery> GetInnerQueries();

        void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery);

        ISparqlQuery FinalizeAfterTransform();
    }

    public interface ISparqlQueryPart : ISparqlQuery
    {
        
    }

    public interface ISparqlQueryModifier : ISparqlQuery
    {
        ISparqlQuery InnerQuery { get; }
    }
}
