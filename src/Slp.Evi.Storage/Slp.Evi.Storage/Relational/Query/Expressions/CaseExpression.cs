using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Statements = statements.ToArray();
            SqlType = Statements.First().Expression.SqlType; // TODO: Better find SQL type
            UsedCalculusVariables =
                Statements.SelectMany(x => x.Expression.UsedCalculusVariables.Union(x.Condition.UsedCalculusVariables))
                    .Distinct()
                    .ToArray();
        }

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>The statements.</value>
        public IEnumerable<Statement> Statements { get; }

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
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }

        /// <inheritdoc />
        public bool HasAlwaysTheSameValue
        {
            get
            {
                if (Statements.Select(x => x.Expression).All(x => x is NullExpression))
                    return true;

                if (Statements.Select(x => x.Expression).All(x => x is ConstantExpression))
                {
                    var constants = Statements.Select(x => x.Expression).Cast<ConstantExpression>().ToList();
                    var sqlTypes = constants.Select(x => x.SqlType).Distinct();
                    var sqlStrings = constants.Select(x => x.SqlString).Distinct();

                    if (sqlTypes.Count() == 1 && sqlStrings.Count() == 1)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

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
            public IFilterCondition Condition { get; }

            /// <summary>
            /// Gets the expression.
            /// </summary>
            /// <value>The expression.</value>
            public IExpression Expression { get; }
        }
    }
}
