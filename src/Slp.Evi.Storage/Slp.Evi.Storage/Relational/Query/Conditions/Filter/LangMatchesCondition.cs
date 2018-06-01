using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// The LangMatches condition
    /// </summary>
    public class LangMatchesCondition
        : IFilterCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LangMatchesCondition"/> class.
        /// </summary>
        /// <param name="languageExpression">The language expression.</param>
        /// <param name="languageRangeExpression">The language range expression.</param>
        public LangMatchesCondition(IExpression languageExpression, IExpression languageRangeExpression)
        {
            LanguageExpression = languageExpression;
            LanguageRangeExpression = languageRangeExpression;
            UsedCalculusVariables = LanguageExpression.UsedCalculusVariables
                .Union(LanguageRangeExpression.UsedCalculusVariables).Distinct().ToList();
        }

        /// <summary>
        /// Gets the language expression (left operand).
        /// </summary>
        public IExpression LanguageExpression { get; }

        /// <summary>
        /// Gets the language range expression (right operand).
        /// </summary>
        public IExpression LanguageRangeExpression { get; }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public object Accept(IFilterConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <inheritdoc />
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }
    }
}
