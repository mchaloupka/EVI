using System.Collections.Generic;
using System.Linq;

namespace Slp.r2rml4net.Storage.Common.Optimization.PatternMatching
{
    /// <summary>
    /// The result of pattern matching.
    /// </summary>
    public class CompareResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareResult"/> class.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        public CompareResult(IEnumerable<MatchCondition> conditions)
        {
            Conditions = conditions.ToArray();
            CanMatch = !Conditions.Any(x => x.IsAlwaysFalse);
            AlwaysMatch = Conditions.Length == 0;
        }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>The conditions.</value>
        public MatchCondition[] Conditions { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance can match.
        /// </summary>
        /// <value><c>true</c> if this instance can match; otherwise, <c>false</c>.</value>
        public bool CanMatch { get; }

        /// <summary>
        /// Gets a value indicating whether this instance always matches.
        /// </summary>
        /// <value><c>true</c> if this instance always matches; otherwise, <c>false</c>.</value>
        public bool AlwaysMatch { get; }
    }
}