using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Query;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// Representation of the base value binder
    /// </summary>
    public class BaseValueBinder
        : IValueBinder
    {
        /// <summary>
        /// The needed variables
        /// </summary>
        private Dictionary<string, ICalculusVariable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="termMap">The term map.</param>
        /// <param name="source">The source.</param>
        public BaseValueBinder(string variableName, ITermMap termMap, ISqlCalculusSource source)
        {
            VariableName = variableName;
            TermMap = termMap;
            _variables = new Dictionary<string, ICalculusVariable>();
            
            if (termMap.IsConstantValued)
            {
                // No columns needed
            }
            else if (termMap.IsColumnValued)
            {
                _variables.Add(termMap.ColumnName, source.GetVariable(termMap.ColumnName));
            }
            else if (termMap.IsTemplateValued)
            {
                var template = termMap.Template;

                var templateProcessor = new TemplateProcessor();
                TemplateParts = templateProcessor.ParseTemplate(template).ToArray();

                foreach (var part in TemplateParts.Where(x => x.IsColumn))
                {
                    _variables.Add(part.Column, source.GetVariable(part.Column));
                }
            }
            else
            {
                throw new Exception("Mapping can be only constant, column or template valued");
            }
        }

        /// <summary>
        /// Gets the term map.
        /// </summary>
        /// <value>The term map.</value>
        public ITermMap TermMap { get; private set; }

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
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; private set; }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        public IEnumerable<ICalculusVariable> NeededCalculusVariables
        {
            get { return _variables.Values; }
        }

        /// <summary>
        /// Gets the template parts.
        /// </summary>
        /// <value>The template parts.</value>
        public IEnumerable<ITemplatePart> TemplateParts { get; private set; } 

        /// <summary>
        /// Gets the calculus variable.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public ICalculusVariable GetCalculusVariable(string columnName)
        {
            return _variables[columnName];
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