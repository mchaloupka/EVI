using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Common.Optimization.PatternMatching
{
    /// <summary>
    /// Class representing pattern in pattern matching.
    /// </summary>
    public class Pattern
    {
        /// <summary>
        /// Gets a value indicating whether this instance is iri escaped.
        /// </summary>
        /// <value><c>true</c> if this instance is iri escaped; otherwise, <c>false</c>.</value>
        public bool IsIriEscaped { get; private set; }

        /// <summary>
        /// Gets the pattern items.
        /// </summary>
        public PatternItem[] PatternItems { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="isIriEscaped">if set to <c>true</c> the pattern is iri escaped.</param>
        /// <param name="patternItems">The pattern items.</param>
        public Pattern(bool isIriEscaped, IEnumerable<PatternItem> patternItems)
        {
            IsIriEscaped = isIriEscaped;
            PatternItems = patternItems.ToArray();
        }
    }
}
