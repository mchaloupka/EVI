using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Source
{
    /// <summary>
    /// The source condition representing left join
    /// </summary>
    public class LeftJoinCondition
        : ISourceCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftJoinCondition"/> class.
        /// </summary>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="joinConditions">The join conditions.</param>
        /// <param name="calculusVariables">The calculus variables.</param>
        public LeftJoinCondition(ICalculusSource rightOperand,
            IEnumerable<IFilterCondition> joinConditions, IEnumerable<ICalculusVariable> calculusVariables)
        {
            RightOperand = rightOperand;
            CalculusVariables = calculusVariables;
            JoinConditions = joinConditions.ToArray();
        }

        /// <summary>
        /// Gets the join conditions.
        /// </summary>
        public IFilterCondition[] JoinConditions { get; private set; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        public ICalculusSource RightOperand { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        public object Accept(ISourceConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the calculus variables.
        /// </summary>
        public IEnumerable<ICalculusVariable> CalculusVariables { get; }
    }
}
