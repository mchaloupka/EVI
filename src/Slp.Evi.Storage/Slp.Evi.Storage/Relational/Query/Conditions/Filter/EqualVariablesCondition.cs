using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// Class representing condition: equal for two <see cref="ICalculusVariable"/>
    /// </summary>
    public class EqualVariablesCondition
        : IFilterCondition
    {
        /// <summary>
        /// Gets the left variable.
        /// </summary>
        /// <value>The left variable.</value>
        public ICalculusVariable LeftVariable { get; }

        /// <summary>
        /// Gets the right variable.
        /// </summary>
        /// <value>The right variable.</value>
        public ICalculusVariable RightVariable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualVariablesCondition"/> class.
        /// </summary>
        /// <param name="leftVariable">The left variable.</param>
        /// <param name="rightVariable">The right variable.</param>
        public EqualVariablesCondition(ICalculusVariable leftVariable, ICalculusVariable rightVariable)
        {
            LeftVariable = leftVariable;
            RightVariable = rightVariable;

            if (LeftVariable != RightVariable)
            {
                UsedCalculusVariables = new[] {LeftVariable, RightVariable};
            }
            else
            {
                UsedCalculusVariables = new ICalculusVariable[] {LeftVariable};
            }
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IFilterConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }
    }
}