using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Types;
using TCode.r2rml4net;
using TCode.r2rml4net.Extensions;
using VDS.RDF;

namespace Slp.Evi.Storage.Relational.Query.ValueBinders
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
        private readonly Dictionary<string, ICalculusVariable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="termMap">The term map.</param>
        /// <param name="source">The source.</param>
        /// <param name="typeCache">The type cache.</param>
        public BaseValueBinder(string variableName, ITermMapping termMap, ISqlCalculusSource source, ITypeCache typeCache)
        {
            VariableName = variableName;
            TermMap = termMap;
            _variables = new Dictionary<string, ICalculusVariable>();
            Type = typeCache.GetValueType(termMap);

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

                _variables = TemplateParts.Where(x => x.IsColumn).Select(x => x.Column).Distinct()
                    .ToDictionary(x => x, source.GetVariable);
            }
            else
            {
                throw new Exception("Mapping can be only constant, column or template valued");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValueBinder"/> class by copying another <see cref="BaseValueBinder"/>
        /// while replacing some of its columns
        /// </summary>
        /// <param name="baseValueBinder">The other base value binder.</param>
        /// <param name="calculusVariableSelection">The calculus variable selection function.</param>
        public BaseValueBinder(BaseValueBinder baseValueBinder, Func<ICalculusVariable, ICalculusVariable> calculusVariableSelection)
        {
            VariableName = baseValueBinder.VariableName;
            TermMap = baseValueBinder.TermMap;
            TemplateParts = baseValueBinder.TemplateParts;
            Type = baseValueBinder.Type;

            _variables = new Dictionary<string, ICalculusVariable>();

            foreach (var variableName in baseValueBinder._variables.Keys)
            {
                _variables.Add(variableName, calculusVariableSelection(baseValueBinder._variables[variableName]));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValueBinder"/> class by copying another <see cref="BaseValueBinder"/>
        /// while replacing some of its columns
        /// </summary>
        /// <param name="baseValueBinder">The other base value binder.</param>
        /// <param name="calculusVariableSelection">The calculus variable selection function.</param>
        public BaseValueBinder(BaseValueBinder baseValueBinder, Func<string, ICalculusVariable> calculusVariableSelection)
        {
            VariableName = baseValueBinder.VariableName;
            TermMap = baseValueBinder.TermMap;
            TemplateParts = baseValueBinder.TemplateParts;
            Type = baseValueBinder.Type;

            _variables = new Dictionary<string, ICalculusVariable>();

            foreach (var variableName in baseValueBinder._variables.Keys)
            {
                _variables.Add(variableName, calculusVariableSelection(variableName));
            }
        }

        /// <summary>
        /// Gets the term map.
        /// </summary>
        /// <value>The term map.</value>
        public ITermMapping TermMap { get; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        public IEnumerable<ICalculusVariable> NeededCalculusVariables => _variables.Values.Distinct();

        /// <summary>
        /// Gets the template parts.
        /// </summary>
        /// <value>The template parts.</value>
        public IEnumerable<ITemplatePart> TemplateParts { get; }

        /// <summary>
        /// The type
        /// </summary>
        public IValueType Type { get; }

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

        /// <summary>
        /// Gets the calculus variable.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <exception cref="System.Exception">Cannot get variable that is not requested for evaluation</exception>
        public ICalculusVariable GetVariable(string columnName)
        {
            if (_variables.ContainsKey(columnName))
            {
                return _variables[columnName];
            }
            else
            {
                throw new Exception("Cannot get variable that is not requested for evaluation");
            }
        }

        /// <summary>
        /// Loads the node.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        public INode LoadNode(INodeFactory nodeFactory, IQueryResultRow rowData, IQueryContext context)
        {
            if (TermMap.IsConstantValued)
            {
                return LoadNodeFromConstant(nodeFactory);
            }
            else if (TermMap.IsColumnValued)
            {
                return LoadNodeFromColumn(nodeFactory, rowData, context);
            }
            else if (TermMap.IsTemplateValued)
            {
                return LoadNodeFromTemplate(nodeFactory, rowData, context);
            }
            else
            {
                throw new Exception("Term map must be either constant, column or template valued");
            }
        }

        /// <summary>
        /// Loads the node from template.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        private INode LoadNodeFromTemplate(INodeFactory nodeFactory, IQueryResultRow rowData, IQueryContext context)
        {
            var value = ReplaceColumnReferences(rowData, TermMap.TermType.IsIri, context);
            return TermForValue(nodeFactory, value, context);
        }

        /// <summary>
        /// Replaces the column references.
        /// </summary>
        /// <param name="rowData">The row data.</param>
        /// <param name="isIri">if set to <c>true</c> the value is iri.</param>
        /// <param name="context">The context.</param>
        private string ReplaceColumnReferences(IQueryResultRow rowData, bool isIri, IQueryContext context)
        {
            var sb = new StringBuilder();
            foreach (var templatePart in TemplateParts)
            {
                if (templatePart.IsText)
                {
                    sb.Append(templatePart.Text);
                }
                else if (templatePart.IsColumn)
                {
                    var variable = GetVariable(templatePart.Column);
                    var replaced = ReplaceColumnReference(rowData, variable, isIri, context);
                    if (replaced == null)
                    {
                        return null;
                    }

                    sb.Append(replaced);
                }
                else
                {
                    throw new Exception("Template part has to be either text or column");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replaces a single column reference.
        /// </summary>
        /// <param name="rowData">The row data.</param>
        /// <param name="variable">The variable.</param>
        /// <param name="isIri">if set to <c>true</c> the value is iri.</param>
        /// <param name="context">The context.</param>
        private string ReplaceColumnReference(IQueryResultRow rowData, ICalculusVariable variable, bool isIri, IQueryContext context)
        {
            var columnName = context.QueryNamingHelpers.GetVariableName(null, variable);
            var dbCol = rowData.GetColumn(columnName);
            var value = dbCol.StringValue;

            if (value == null)
            {
                return null;
            }

            return isIri ? MappingHelper.UrlEncode(value) : value;
        }

        /// <summary>
        /// Creates term for value.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The context.</param>
        private INode TermForValue(INodeFactory nodeFactory, string value, IQueryContext context)
        {
            if (value == null)
            {
                return null;
            }

            if (TermMap.TermType.IsIri)
            {
                return UriTermForValue(value, nodeFactory, TermMap.BaseIri);
            }
            else if (TermMap.TermType.IsBlankNode)
            {
                return BlankNodeForValue(value, nodeFactory, context);
            }
            else if (TermMap.TermType.IsLiteral)
            {
                return TermForLiteral(value, nodeFactory);
            }
            else
            {
                throw new Exception("Unhandled term type");
            }
        }

        /// <summary>
        /// Creates the URI term for value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="baseUri">The base URI.</param>
        private static INode UriTermForValue(string value, INodeFactory factory, Uri baseUri)
        {
            try
            {
                var uri = new Uri(value, UriKind.RelativeOrAbsolute);

                if (!uri.IsAbsoluteUri)
                {
                    uri = ConstructAbsoluteUri(value, baseUri);
                }

                if (uri.IsAbsoluteUri)
                {
                    uri.LeaveDotsAndSlashesEscaped();
                    return factory.CreateUriNode(uri);
                }
                else
                {
                    throw new Exception("Now the uri must be absolute");
                }
            }
            catch (Exception)
            {
                throw new Exception($"Value {value} is invalid uri");
            }
        }

        /// <summary>
        /// Constructs the absolute URI.
        /// </summary>
        /// <param name="relativePart">The relative part.</param>
        /// <param name="baseUri">The base URI.</param>
        private static Uri ConstructAbsoluteUri(string relativePart, Uri baseUri)
        {
            if (relativePart.Split('/').Any(seg => seg == "." || seg == ".."))
                throw new Exception("The relative IRI cannot contain any . or .. parts");

            return new Uri(baseUri + relativePart);
        }

        /// <summary>
        /// Creates the blank node for value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="context">The context.</param>
        private INode BlankNodeForValue(string value, INodeFactory nodeFactory, IQueryContext context)
        {
            return context.GetBlankNodeForValue(nodeFactory, value);
        }

        /// <summary>
        /// Creates the term for literal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="nodeFactory">The node factory.</param>
        private INode TermForLiteral(string value, INodeFactory nodeFactory)
        {
            if(!(Type is ILiteralValueType literalValueType))
                throw new InvalidOperationException("It is not possible to generate literal for non literal type");

            return literalValueType.CreateLiteralNode(nodeFactory, value);
        }

        /// <summary>
        /// Loads the node from constant.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        private INode LoadNodeFromConstant(INodeFactory nodeFactory)
        {
            if (TermMap.Iri != null)
            {
                return nodeFactory.CreateUriNode(TermMap.Iri);
            }
            else if (TermMap is IObjectMapping objectMapping)
            {
                if (objectMapping.Literal != null)
                {
                    var value = objectMapping.Literal.Value;
                    return ((ILiteralValueType) Type).CreateLiteralNode(nodeFactory, value);
                }
                else
                {
                    throw new Exception("Object map's value must be IRI or literal.");
                }
            }
            else
            {
                throw new Exception("Constant must be IRI valued or an object map");
            }
        }

        /// <summary>
        /// Loads the node from column.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        private INode LoadNodeFromColumn(INodeFactory nodeFactory, IQueryResultRow rowData, IQueryContext context)
        {
            var column = NeededCalculusVariables.Single();
            var columnName = context.QueryNamingHelpers.GetVariableName(null, column);
            var dbCol = rowData.GetColumn(columnName);
            var val = dbCol.StringValue;

            if (TermMap.TermType.IsLiteral)
            {
                return TermForLiteral(val, nodeFactory);
            }
            else if (TermMap.TermType.IsIri)
            {
                AssertNoIllegalCharacters(new Uri(val));
                return UriTermForValue(val, nodeFactory, TermMap.BaseIri);
            }
            else
            {
                throw new Exception("Cannot generate blank node from column mapping");
            }
        }

        /// <summary>
        /// Asserts the no illegal characters.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception"></exception>
        private static void AssertNoIllegalCharacters(Uri value)
        {
            var disallowedChars = new List<char>();
            var segments = value.IsAbsoluteUri ? value.Segments : new[] { value.OriginalString };

            foreach (var segment in segments)
            {
                if (segment.Any(chara => !MappingHelper.IsIUnreserved(chara)))
                {
                    disallowedChars.AddRange(segment.Where(chara => chara != '/' && !MappingHelper.IsIUnreserved(chara)));
                }
            }

            if (disallowedChars.Count > 0)
            {
                throw new Exception(
                    $"Column value is not escaped and thus cannot contain these disallowed characters: {string.Join(",", disallowedChars.Distinct().Select(c => $"'{c}'"))}");
            }
        }
    }
}