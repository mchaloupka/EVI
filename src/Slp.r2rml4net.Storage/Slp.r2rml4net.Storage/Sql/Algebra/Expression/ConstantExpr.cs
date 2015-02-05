using System;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    /// <summary>
    /// Constant expression.
    /// </summary>
    public class ConstantExpr : IExpression
    {
        // TODO: Value escaping
        // TODO: Connect with current db vendor

        /// <summary>
        /// Gets the SQL string.
        /// </summary>
        /// <value>The SQL string.</value>
        public string SqlString { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpr"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public ConstantExpr(Uri uri)
        {
            SqlString = string.Format("\'{0}\'", uri.AbsoluteUri);
            Value = uri;
            // TODO: Set SQL type
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpr"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public ConstantExpr(string text)
        {
            SqlString = string.Format("\'{0}\'", text);
            Value = text;
            // TODO: Set SQL type
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpr"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        public ConstantExpr(int number)
        {
            Value = number;
            SqlString = number.ToString();
            // TODO: Set SQL type
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
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object Clone()
        {
            if (Value is Uri)
            {
                return new ConstantExpr((Uri)Value);
            }
            else if (Value is int)
            {
                return new ConstantExpr((int)Value);
            }
            else if (Value is string)
            {
                return new ConstantExpr((string)Value);
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; private set; }
    }
}
