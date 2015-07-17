using System;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;

namespace Slp.r2rml4net.Storage.Relational.Query.Expression
{
    /// <summary>
    /// Class ConstantExpression.
    /// </summary>
    public class ConstantExpression 
        : IExpression
    {
        /// <summary>
        /// The context
        /// </summary>
        private QueryContext _context;

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
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="context">Query context</param>
        public ConstantExpression(Uri uri, QueryContext context)
        {
            SqlString = string.Format("\'{0}\'", uri.AbsoluteUri);
            Value = uri;
            _context = context;
            SqlType = context.Db.SqlTypeForString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// /// <param name="context">Query context</param>
        public ConstantExpression(string text, QueryContext context)
        {
            SqlString = string.Format("\'{0}\'", text);
            Value = text;
            _context = context;
            SqlType = context.Db.SqlTypeForString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// /// <param name="context">Query context</param>
        public ConstantExpression(int number, QueryContext context)
        {
            Value = number;
            SqlString = number.ToString();
            _context = context;
            SqlType = context.Db.SqlTypeForInt;
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; private set; }

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
    }
}