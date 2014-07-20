using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    /// <summary>
    /// Value binder that never returns a value
    /// </summary>
    public class BlankValueBinder : IBaseValueBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlankValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        public BlankValueBinder(string variableName)
        {
            this.VariableName = variableName;
        }

        /// <summary>
        /// Loads the node value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="row">The db row.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The node.</returns>
        public VDS.RDF.INode LoadNode(VDS.RDF.INodeFactory factory, IQueryResultRow row, Query.QueryContext context)
        {
            return null;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; private set; }

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        public IEnumerable<Algebra.ISqlColumn> AssignedColumns
        {
            get { yield break; }
        }

        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public void ReplaceAssignedColumn(Algebra.ISqlColumn oldColumn, Algebra.ISqlColumn newColumn)
        {
            
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new BlankValueBinder(this.VariableName);
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
