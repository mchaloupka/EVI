using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public BinaryNumericExpression(IExpression leftOperand, IExpression rightOperand, ArithmeticOperation oper, IQueryContext context)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operator = oper;
            SqlType = context.Db.GetCommonTypeForComparison(leftOperand.SqlType, rightOperand.SqlType);
            HasAlwaysTheSameValue = leftOperand.HasAlwaysTheSameValue && rightOperand.HasAlwaysTheSameValue;
            UsedCalculusVariables = leftOperand.UsedCalculusVariables.Union(rightOperand.UsedCalculusVariables)
                .Distinct().ToArray();
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public object Accept(IExpressionVisitor visitor, object data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public DataType SqlType { get; }

        /// <inheritdoc />
        public IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }

        /// <inheritdoc />
        public bool HasAlwaysTheSameValue { get; }
    }
}
