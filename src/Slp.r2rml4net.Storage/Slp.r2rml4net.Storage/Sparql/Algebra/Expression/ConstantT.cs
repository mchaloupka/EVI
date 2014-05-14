using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Nodes;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Expression
{
    public class ConstantT : ISparqlQueryExpression
    {
        public ConstantT(IValuedNode node)
        {
            this.Node = node;
        }

        public IValuedNode Node { get; private set; }
    }
}
