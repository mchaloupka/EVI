using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Nodes;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Expression
{
    /// <summary>
    /// Constant expression.
    /// </summary>
    public class ConstantT : ISparqlQueryExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantT"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public ConstantT(IValuedNode node)
        {
            this.Node = node;
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>The node.</value>
        public IValuedNode Node { get; private set; }
    }
}
