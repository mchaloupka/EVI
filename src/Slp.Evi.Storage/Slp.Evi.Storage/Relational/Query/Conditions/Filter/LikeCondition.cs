using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// Represents the LIKE operator.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.IFilterCondition" />
    public class LikeCondition
        : IFilterCondition
    {
        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public IExpression Expression { get; }

        /// <summary>
        /// Gets the pattern to match.
        /// </summary>
        /// <value>The pattern.</value>
        public string Pattern { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LikeCondition"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="pattern">The pattern.</param>
        public LikeCondition(IExpression expression, string pattern)
        {
            Expression = expression;
            Pattern = pattern;

            UsedCalculusVariables = Expression.UsedCalculusVariables;
        }

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
