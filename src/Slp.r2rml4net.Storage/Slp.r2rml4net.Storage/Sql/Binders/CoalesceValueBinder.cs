using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    /// <summary>
    /// COALESCE value binder.
    /// </summary>
    public class CoalesceValueBinder : IBaseValueBinder
    {
        /// <summary>
        /// The binders
        /// </summary>
        private List<IBaseValueBinder> binders;

        /// <summary>
        /// Prevents a default instance of the <see cref="CoalesceValueBinder"/> class from being created.
        /// </summary>
        private CoalesceValueBinder()
        {
            this.binders = new List<IBaseValueBinder>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoalesceValueBinder"/> class.
        /// </summary>
        /// <param name="originalValueBinder">The original value binder.</param>
        public CoalesceValueBinder(IBaseValueBinder originalValueBinder)
        {
            this.binders = new List<IBaseValueBinder>();

            if (originalValueBinder is CoalesceValueBinder)
                this.binders.AddRange(((CoalesceValueBinder)originalValueBinder).binders);
            else
                this.binders.Add(originalValueBinder);
        }

        /// <summary>
        /// Adds the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        /// <exception cref="System.Exception">Cannot collate value binders for different variables</exception>
        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            if (valueBinder.VariableName != VariableName)
                throw new Exception("Cannot collate value binders for different variables");

            if (valueBinder is CoalesceValueBinder)
                this.binders.AddRange(((CoalesceValueBinder)valueBinder).binders);
            else
                this.binders.Add(valueBinder);
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
            foreach (var binder in this.binders)
            {
                var node = binder.LoadNode(factory, row, context);

                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName
        {
            get { return this.binders[0].VariableName; }
        }

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { return this.binders.SelectMany(x => x.AssignedColumns).Distinct(); }
        }

        /// <summary>
        /// Gets the inner binders.
        /// </summary>
        /// <value>The inner binders.</value>
        public IEnumerable<IBaseValueBinder> InnerBinders { get { return binders; } }


        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            foreach (var binder in binders)
            {
                binder.ReplaceAssignedColumn(oldColumn, newColumn);
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            var newBinder = new CoalesceValueBinder();

            foreach (var binder in this.InnerBinders)
            {
                newBinder.binders.Add((IBaseValueBinder)binder.Clone());
            }

            return newBinder;
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
        /// Replaces the value binder.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="newBinder">The new binder.</param>
        public void ReplaceValueBinder(IBaseValueBinder binder, IBaseValueBinder newBinder)
        {
            var index = this.binders.IndexOf(binder);

            if (index > -1)
                this.binders[index] = newBinder;
        }

        /// <summary>
        /// Removes the value binder.
        /// </summary>
        /// <param name="binder">The binder.</param>
        public void RemoveValueBinder(IBaseValueBinder binder)
        {
            var index = this.binders.IndexOf(binder);

            if (index > -1)
                this.binders.RemoveAt(index);
        }
    }
}
