using System.Collections.Generic;
using System.Diagnostics;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    /// <summary>
    /// SQL side value binder.
    /// </summary>
    public class SqlSideValueBinder : IBaseValueBinder
    {
        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>The column.</value>
        public ISqlColumn Column { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSideValueBinder"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="originalBinder">The original binder.</param>
        public SqlSideValueBinder(ISqlColumn column, IBaseValueBinder originalBinder)
        {
            OriginalBinder = originalBinder;
            Column = column;
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
            var value = row.GetColumn(Column.Name).Value;

            if (value == null)
                return null;

            return factory.CreateLiteralNode(value.ToString());
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get { return OriginalBinder.VariableName; } }

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { yield return Column; }
        }

        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            if (oldColumn == Column)
                Column = newColumn;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new SqlSideValueBinder(Column, OriginalBinder);
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

        /// <summary>
        /// Gets the original binder.
        /// </summary>
        /// <value>The original binder.</value>
        public IBaseValueBinder OriginalBinder { get; private set; }
    }
}
