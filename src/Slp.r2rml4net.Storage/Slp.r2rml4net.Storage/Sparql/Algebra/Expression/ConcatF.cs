using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Expression
{
    public class ConcatF : ISparqlQueryExpression
    {
        private List<ISparqlQueryExpression> parts;
        
        public ConcatF(IEnumerable<ISparqlQueryExpression> parts)
        {
            this.parts = parts.ToList();
        }

        public IEnumerable<ISparqlQueryExpression> Parts { get { return this.parts; } }
    }
}
