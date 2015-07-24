using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Query;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// COALESCE value binder
    /// </summary>
    public class CoalesceValueBinder : IValueBinder
    {
        /// <summary>
        /// The coalesced value binders
        /// </summary>
        private readonly IValueBinder[] _valueBinders;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="valueBinders"></param>
        public CoalesceValueBinder(string variableName, params IValueBinder[] valueBinders)
        {
            _valueBinders = valueBinders;
            VariableName = variableName;
        }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        public IEnumerable<ICalculusVariable> NeededCalculusVariables
        {
            get { return _valueBinders.SelectMany(x => x.NeededCalculusVariables).Distinct(); }
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; }

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
        /// Loads the node.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        public INode LoadNode(INodeFactory nodeFactory, IQueryResultRow rowData, QueryContext context)
        {
            return _valueBinders.Select(valueBinder => valueBinder.LoadNode(nodeFactory, rowData, context)).FirstOrDefault(node => node != null);
        }
    }
}