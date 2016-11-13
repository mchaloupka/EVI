using System;
using System.Collections.Generic;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Query;

namespace Slp.Evi.Storage.Relational.Query.Expressions
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
        private IQueryContext _context;

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
        public ConstantExpression(Uri uri, IQueryContext context)
        {
            SqlString = $"\'{uri.AbsoluteUri}\'";
            Value = uri.AbsoluteUri;
            _context = context;
            SqlType = context.Db.SqlTypeForString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// /// <param name="context">Query context</param>
        public ConstantExpression(string text, IQueryContext context)
        {
            SqlString = $"\'{text}\'";
            Value = text;
            _context = context;
            SqlType = context.Db.SqlTypeForString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// /// <param name="context">Query context</param>
        public ConstantExpression(int number, IQueryContext context)
        {
            Value = number;
            SqlString = number.ToString();
            _context = context;
            SqlType = context.Db.SqlTypeForInt;
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; }

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
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        public IEnumerable<ICalculusVariable> UsedCalculusVariables
        {
            get { yield break; }
        }
    }
}