using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.Query.Expressions
{
    /// <summary>
    /// Class representing a <c>NULL</c> expression.
    /// </summary>
    public class NullExpression
        : IExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullExpression"/> class.
        /// </summary>
        /// <param name="sqlType">The SQL type for this expression.</param>
        public NullExpression(DataType sqlType)
        {
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
        public IEnumerable<ICalculusVariable> UsedCalculusVariables => Enumerable.Empty<ICalculusVariable>();

        /// <inheritdoc />
        public bool HasAlwaysTheSameValue => true;
    }
}
