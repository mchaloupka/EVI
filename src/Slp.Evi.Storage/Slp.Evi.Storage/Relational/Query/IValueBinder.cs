using System.Collections.Generic;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Utils;
using VDS.RDF;

namespace Slp.Evi.Storage.Relational.Query
{
    /// <summary>
    /// Value binder
    /// </summary>
    public interface IValueBinder
        : IVisitable<IValueBinderVisitor>
    {
        /// <summary>
        /// Loads the node.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        INode LoadNode(INodeFactory nodeFactory, IQueryResultRow rowData, QueryContext context);

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        string VariableName { get; }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        IEnumerable<ICalculusVariable> NeededCalculusVariables { get; }
    }
}
