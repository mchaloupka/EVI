using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private readonly List<IBaseValueBinder> _binders;

        /// <summary>
        /// Prevents a default instance of the <see cref="CoalesceValueBinder"/> class from being created.
        /// </summary>
        private CoalesceValueBinder()
        {
            _binders = new List<IBaseValueBinder>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoalesceValueBinder"/> class.
        /// </summary>
        /// <param name="originalValueBinder">The original value binder.</param>
        public CoalesceValueBinder(IBaseValueBinder originalValueBinder)
        {
            _binders = new List<IBaseValueBinder>();

            if (originalValueBinder is CoalesceValueBinder)
                _binders.AddRange(((CoalesceValueBinder)originalValueBinder)._binders);
            else
                _binders.Add(originalValueBinder);
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
                _binders.AddRange(((CoalesceValueBinder)valueBinder)._binders);
            else
                _binders.Add(valueBinder);
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
            foreach (var binder in _binders)
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
            get { return _binders[0].VariableName; }
        }

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { return _binders.SelectMany(x => x.AssignedColumns).Distinct(); }
        }

        /// <summary>
        /// Gets the inner binders.
        /// </summary>
        /// <value>The inner binders.</value>
        public IEnumerable<IBaseValueBinder> InnerBinders { get { return _binders; } }


        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            foreach (var binder in _binders)
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

            foreach (var binder in InnerBinders)
            {
                newBinder._binders.Add((IBaseValueBinder)binder.Clone());
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
            var index = _binders.IndexOf(binder);

            if (index > -1)
                _binders[index] = newBinder;
        }

        /// <summary>
        /// Removes the value binder.
        /// </summary>
        /// <param name="binder">The binder.</param>
        public void RemoveValueBinder(IBaseValueBinder binder)
        {
            var index = _binders.IndexOf(binder);

            if (index > -1)
                _binders.RemoveAt(index);
        }
    }
}
