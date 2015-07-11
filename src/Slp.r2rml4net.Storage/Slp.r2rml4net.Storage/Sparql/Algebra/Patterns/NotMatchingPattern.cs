using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Pattern that is not matching anything
    /// </summary>
    public class NotMatchingPattern
        : IGraphPattern
    {
        /// <summary>
        /// Constructs an instance of <see cref="NotMatchingPattern"/>
        /// </summary>
        /// <param name="variables">List of variables used</param>
        public NotMatchingPattern(IEnumerable<string> variables)
        {
            Variables = variables.ToArray();
        }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IPatternVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
