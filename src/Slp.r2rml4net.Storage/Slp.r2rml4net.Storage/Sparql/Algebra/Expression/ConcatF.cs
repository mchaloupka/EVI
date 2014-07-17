using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Expression
{
    /// <summary>
    /// Concat expression.
    /// </summary>
    public class ConcatF : ISparqlQueryExpression
    {
        /// <summary>
        /// The parts
        /// </summary>
        private List<ISparqlQueryExpression> parts;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatF"/> class.
        /// </summary>
        /// <param name="parts">The parts.</param>
        public ConcatF(IEnumerable<ISparqlQueryExpression> parts)
        {
            this.parts = parts.ToList();
        }

        /// <summary>
        /// Gets the parts.
        /// </summary>
        /// <value>The parts.</value>
        public IEnumerable<ISparqlQueryExpression> Parts { get { return this.parts; } }
    }
}
