﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    /// <summary>
    /// OR Condition
    /// </summary>
    public class OrCondition : ICondition
    {
        /// <summary>
        /// The conditions
        /// </summary>
        private List<ICondition> conditions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrCondition"/> class.
        /// </summary>
        public OrCondition()
        {
            this.conditions = new List<ICondition>();
        }

        /// <summary>
        /// Adds to condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public void AddToCondition(ICondition condition)
        {
            conditions.Add(condition);
        }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>The conditions.</value>
        public IEnumerable<ICondition> Conditions { get { return conditions; } }

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
            var orCondition = new OrCondition();

            foreach (var cond in this.conditions)
            {
                orCondition.conditions.Add((ICondition)cond.Clone());
            }

            return orCondition;
        }
    }
}
