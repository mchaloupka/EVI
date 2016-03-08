using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions.Assignment
{
    /// <summary>
    /// The assignment from expression condition
    /// </summary>
    public class AssignmentFromExpressionCondition
        : IAssignmentCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentFromExpressionCondition"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="expression">The expression.</param>
        public AssignmentFromExpressionCondition(ICalculusVariable variable, IExpression expression)
        {
            this.Variable = variable;
            this.Expression = expression;
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>The variable.</value>
        public ICalculusVariable Variable { get; private set; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IAssignmentConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
