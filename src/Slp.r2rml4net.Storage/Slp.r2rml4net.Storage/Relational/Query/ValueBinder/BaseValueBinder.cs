using System;
using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Database;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Relational.Query.ValueBinder
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
        /// The _template parts
        /// </summary>
        private ITemplatePart[] _templateParts;

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
                _templateParts = templateProcessor.ParseTemplate(template).ToArray();

                foreach (var part in _templateParts.Where(x => x.IsColumn))
                {
                    _variables.Add(part.Column, source.GetVariable(part.Column));
                }
            }
            else
            {
                throw new NotImplementedException();
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
    }
}