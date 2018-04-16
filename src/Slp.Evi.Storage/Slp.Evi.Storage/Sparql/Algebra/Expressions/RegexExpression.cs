using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// Represents the REGEX function.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.ISparqlCondition" />
    public class RegexExpression
        : ISparqlCondition
    {
        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public ISparqlExpression Text { get; }

        /// <summary>
        /// Gets the pattern.
        /// </summary>
        /// <value>The pattern.</value>
        public ISparqlExpression Pattern { get; }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        /// <value>The flags.</value>
        public ISparqlExpression Flags { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexExpression"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="flags">The flags.</param>
        public RegexExpression(ISparqlExpression text, ISparqlExpression pattern, ISparqlExpression flags = null)
        {
            Text = text;
            Pattern = pattern;
            Flags = flags;
            NeededVariables = text.NeededVariables.Union(pattern.NeededVariables).Union(flags?.NeededVariables ?? Enumerable.Empty<string>());
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <inheritdoc />
        public IEnumerable<string> NeededVariables { get; }
    }
}
