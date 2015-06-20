using System;

namespace Slp.r2rml4net.Storage.Relational.Query.Condition
{
    /// <summary>
    /// Class representing condition: equal for two <see cref="ICalculusVariable"/>
    /// </summary>
    public class EqualVariablesCondition : ICondition
    {
        /// <summary>
        /// Gets the left variable.
        /// </summary>
        /// <value>The left variable.</value>
        public ICalculusVariable LeftVariable { get; private set; }

        /// <summary>
        /// Gets the right variable.
        /// </summary>
        /// <value>The right variable.</value>
        public ICalculusVariable RightVariable { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualVariablesCondition"/> class.
        /// </summary>
        /// <param name="leftVariable">The left variable.</param>
        /// <param name="rightVariable">The right variable.</param>
        public EqualVariablesCondition(ICalculusVariable leftVariable, ICalculusVariable rightVariable)
        {
            LeftVariable = leftVariable;
            RightVariable = rightVariable;
        }
    }
}