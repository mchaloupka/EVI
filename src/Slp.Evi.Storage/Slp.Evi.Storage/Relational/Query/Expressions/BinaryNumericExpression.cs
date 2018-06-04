using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;

namespace Slp.Evi.Storage.Relational.Query.Expressions
{
    /// <summary>
    /// Represents a binary numeric expression
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.IExpression" />
    public class BinaryNumericExpression
        : IExpression
    {
        /// <summary>
        /// Gets the left operand.
        /// </summary>
        public IExpression LeftOperand { get; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        public IExpression RightOperand { get; }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        public ArithmeticOperation Operator { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryNumericExpression"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="context">The context.</param>
        public BinaryNumericExpression(IExpression leftOperand, IExpression rightOperand, ArithmeticOperation oper,
            IQueryContext context)
            : this(leftOperand, rightOperand, oper, (DataType)null)
        {
            SqlType = context.Db.GetCommonTypeForTwoColumns(leftOperand.SqlType, rightOperand.SqlType, out _, out _);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryNumericExpression"/> class.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="oper">The operator.</param>
        /// <param name="sqlType">The sql type.</param>
        public BinaryNumericExpression(IExpression leftOperand, IExpression rightOperand, ArithmeticOperation oper, DataType sqlType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operator = oper;
            HasAlwaysTheSameValue = leftOperand.HasAlwaysTheSameValue && rightOperand.HasAlwaysTheSameValue;
            UsedCalculusVariables = leftOperand.UsedCalculusVariables.Union(rightOperand.UsedCalculusVariables)
                .Distinct().ToArray();
            SqlType = sqlType;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <inheritdoc />
        public DataType SqlType { get; }

        /// <inheritdoc />
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }

        /// <inheritdoc />
        public bool HasAlwaysTheSameValue { get; }
    }
}
