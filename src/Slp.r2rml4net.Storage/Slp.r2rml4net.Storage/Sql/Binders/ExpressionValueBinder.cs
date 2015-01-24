using System.Collections.Generic;
using System.Diagnostics;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    /// <summary>
    /// SQL expression value binder.
    /// </summary>
    public class ExpressionValueBinder : IBaseValueBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="expression">The expression.</param>
        public ExpressionValueBinder(string variableName, IExpression expression)
        {
            VariableName = variableName;
            Expression = expression;
        }

        /// <summary>
        /// Loads the node value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="row">The db row.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The node.</returns>
        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            // NOTE: I should work with types
            var value = Expression.StaticEvaluation(row);

            if (value != null)
                return factory.CreateLiteralNode(value.ToString());
            else
                return null;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; private set; }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public IExpression Expression { get; set; }

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { return Expression.GetAllReferencedColumns(); }
        }

        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            Expression.ReplaceColumnReference(oldColumn, newColumn);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new ExpressionValueBinder(VariableName, (IExpression)Expression.Clone());
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
