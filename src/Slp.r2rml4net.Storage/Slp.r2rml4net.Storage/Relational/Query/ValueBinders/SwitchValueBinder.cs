using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Query;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// The switch value binder
    /// </summary>
    public class SwitchValueBinder
        : IValueBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="caseVariable">The case variable.</param>
        /// <param name="cases">The cases.</param>
        public SwitchValueBinder(string variableName, ICalculusVariable caseVariable, IEnumerable<Case> cases)
        {
            VariableName = variableName;
            CaseVariable = caseVariable;
            Cases = cases;
        }

        /// <summary>
        /// Gets the cases.
        /// </summary>
        /// <value>The cases.</value>
        public IEnumerable<Case> Cases { get; private set; }

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the case variable.
        /// </summary>
        /// <value>The case variable.</value>
        public ICalculusVariable CaseVariable { get; private set; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; private set; }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        public IEnumerable<ICalculusVariable> NeededCalculusVariables { get; }

        /// <summary>
        /// Value binder case
        /// </summary>
        public class Case
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Case"/> class.
            /// </summary>
            /// <param name="caseValue">The case value.</param>
            /// <param name="valueBinder">The value binder.</param>
            public Case(int caseValue, IValueBinder valueBinder)
            {
                CaseValue = caseValue;
                ValueBinder = valueBinder;
            }

            /// <summary>
            /// Gets the case value.
            /// </summary>
            /// <value>The case value.</value>
            public int CaseValue { get; private set; }

            /// <summary>
            /// Gets the value binder.
            /// </summary>
            /// <value>The value binder.</value>
            public IValueBinder ValueBinder { get; private set; }
        }
    }
}
