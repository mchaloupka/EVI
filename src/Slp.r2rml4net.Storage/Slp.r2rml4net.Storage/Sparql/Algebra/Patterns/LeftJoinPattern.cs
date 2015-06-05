using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Left join
    /// </summary>
    public class LeftJoinPattern
        : IGraphPattern
    {
        // TODO: Add missing parts

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftJoinPattern"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        public LeftJoinPattern(IGraphPattern leftOperand, IGraphPattern rightOperand)
        {
            this.LeftOperand = leftOperand;
            this.RightOperand = rightOperand;

            this.Variables = this.LeftOperand.Variables
                .Union(this.RightOperand.Variables).Distinct().ToList();
        }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        /// <value>The right operand.</value>
        public IGraphPattern RightOperand { get; private set; }

        /// <summary>
        /// Gets the left operand.
        /// </summary>
        /// <value>The left operand.</value>
        public IGraphPattern LeftOperand { get; private set; }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; private set; }
    }
}
