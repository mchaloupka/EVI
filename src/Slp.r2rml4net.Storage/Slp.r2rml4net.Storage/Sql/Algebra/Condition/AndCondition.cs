﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    /// <summary>
    /// AND condition
    /// </summary>
    public class AndCondition : ICondition
    {
        /// <summary>
        /// The conditions
        /// </summary>
        private readonly List<ICondition> _conditions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndCondition"/> class.
        /// </summary>
        public AndCondition()
        {
            _conditions = new List<ICondition>();
        }

        /// <summary>
        /// Adds to condition.
        /// </summary>
        /// <param name="cond">The condiditon.</param>
        public void AddToCondition(ICondition cond)
        {
            _conditions.Add(cond);
        }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>The conditions.</value>
        public IEnumerable<ICondition> Conditions { get { return _conditions; } }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            var newAnd = new AndCondition();

            foreach (var cond in _conditions)
            {
                newAnd._conditions.Add((ICondition)cond.Clone());
            }

            return newAnd;
        }
    }
}
