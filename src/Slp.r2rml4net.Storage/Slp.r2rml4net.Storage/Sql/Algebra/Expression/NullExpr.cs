using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    /// <summary>
    /// NULL expression.
    /// </summary>
    public class NullExpr : IExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullExpr"/> class.
        /// </summary>
        /// <param name="sqlDataType">The SQL type</param>
        public NullExpr(DataType sqlDataType)
        {
            SqlType = sqlDataType;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new NullExpr(this.SqlType);
        }

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
        public DataType SqlType { get; private set; }
    }
}
