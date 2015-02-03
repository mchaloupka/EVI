using System.Collections.Generic;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    /// <summary>
    /// CASE expression
    /// </summary>
    public class CaseExpr : IExpression
    {
        /// <summary>
        /// The statements
        /// </summary>
        private readonly List<CaseStatementExpression> _statements;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseExpr"/> class.
        /// </summary>
        /// <param name="sqlDataType">The sql type</param>
        public CaseExpr(DataType sqlDataType)
        {
            _statements = new List<CaseStatementExpression>();
            SqlType = sqlDataType;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            var cas = new CaseExpr(SqlType);

            foreach (var item in _statements)
            {
                cas._statements.Add(new CaseStatementExpression((ICondition)item.Condition.Clone(), (IExpression)item.Expression.Clone()));
            }

            return cas;
        }

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>The statements.</value>
        public IEnumerable<CaseStatementExpression> Statements { get { return _statements; } }

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
        /// Adds the statement.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="expression">The expression.</param>
        public void AddStatement(ICondition condition, IExpression expression)
        {
            _statements.Add(new CaseStatementExpression(condition, expression));
        }

        /// <summary>
        /// Removes the statement.
        /// </summary>
        /// <param name="statement">The statement.</param>
        public void RemoveStatement(CaseStatementExpression statement)
        {
            var index = _statements.IndexOf(statement);

            if (index > -1)
                _statements.RemoveAt(index);
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; private set; }
    }

    /// <summary>
    /// CASE statement
    /// </summary>
    public class CaseStatementExpression
    {
        /// <summary>
        /// Gets or sets the condition.
        /// </summary>
        /// <value>The condition.</value>
        public ICondition Condition { get; set; }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public IExpression Expression { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatementExpression"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="expression">The expression.</param>
        public CaseStatementExpression(ICondition condition, IExpression expression)
        {
            Condition = condition;
            Expression = expression;
        }

    }
}
