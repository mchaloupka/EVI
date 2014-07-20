using System;
using System.Collections.Generic;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Utils;
using VDS.RDF;
namespace Slp.r2rml4net.Storage.Sql.Binders
{
    /// <summary>
    /// Base value binder
    /// </summary>
    public interface IBaseValueBinder : ICloneable, IVisitable<IValueBinderVisitor>
    {
        /// <summary>
        /// Loads the node value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="row">The db row.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The node.</returns>
        INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context);

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        string VariableName { get; }

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        IEnumerable<ISqlColumn> AssignedColumns { get; }

        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn);
    }
}
