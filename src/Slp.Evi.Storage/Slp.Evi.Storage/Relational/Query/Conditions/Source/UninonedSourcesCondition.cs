using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions.Source
{
    /// <summary>
    /// The source condition representing union
    /// </summary>
    public class UnionedSourcesCondition 
        : ISourceCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnionedSourcesCondition"/> class.
        /// </summary>
        /// <param name="caseVariable">The case variable.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="sources">The sources.</param>
        public UnionedSourcesCondition(ICalculusVariable caseVariable, IEnumerable<ICalculusVariable> variables, IEnumerable<ICalculusSource> sources)
        {
            CaseVariable = caseVariable;
            CalculusVariables = variables;
            Sources = sources;
        }

        /// <summary>
        /// Gets the calculus variables.
        /// </summary>
        /// <value>The calculus variables.</value>
        public IEnumerable<ICalculusVariable> CalculusVariables { get; private set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public IEnumerable<ICalculusSource> Sources { get; private set; }

        /// <summary>
        /// Gets the case variable.
        /// </summary>
        /// <value>The case variable.</value>
        public ICalculusVariable CaseVariable { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISourceConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
