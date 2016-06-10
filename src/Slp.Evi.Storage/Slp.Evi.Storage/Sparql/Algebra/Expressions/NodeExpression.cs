using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Nodes;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Expression representing the <see cref="IValuedNode"/>.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.ISparqlExpression" />
    public class NodeExpression 
        : ISparqlExpression
    {
        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>The node.</value>
        public IValuedNode Node { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeExpression"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public NodeExpression(IValuedNode node)
        {
            Node = node;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the needed variables to evaluate the expression.
        /// </summary>
        public IEnumerable<string> NeededVariables => new string[] {};
    }
}
