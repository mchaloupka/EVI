using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.Query.Expressions
{
    /// <summary>
    /// Class representing CASE expression
    /// </summary>
    public class CaseExpression
        : IExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseExpression"/> class.
        /// </summary>
        /// <param name="statements">The statements.</param>
        public CaseExpression(IEnumerable<Statement> statements)
        {
            this.Statements = statements;
            this.SqlType = statements.First().Expression.SqlType; // TODO: Better find SQL type
        }

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>The statements.</value>
        public IEnumerable<Statement> Statements { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; }

        /// <summary>
        /// Statement in the case expression
        /// </summary>
        public class Statement
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Statement"/> class.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="expression">The expression.</param>
            public Statement(IFilterCondition condition, IExpression expression)
            {
                Condition = condition;
                Expression = expression;
            }

            /// <summary>
            /// Gets the condition.
            /// </summary>
            /// <value>The condition.</value>
            public IFilterCondition Condition { get; private set; }

            /// <summary>
            /// Gets the expression.
            /// </summary>
            /// <value>The expression.</value>
            public IExpression Expression { get; private set; }
        }
    }
}
