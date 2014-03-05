using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using TCode.r2rml4net.Mapping;
using TCode.r2rml4net.Extensions;
using VDS.RDF;
using TCode.r2rml4net;

// https://bitbucket.org/r2rml4net/core/src/46143a763b43630b1c645e29ec6e4193fc8ada22/src/TCode.r2rml4net/TriplesGeneration/RDFTermGenerator.cs?at=default

// TODO: Read this and implement getting value properly (with type)
// https://bitbucket.org/r2rml4net/core/src/46143a763b43630b1c645e29ec6e4193fc8ada22/src/TCode.r2rml4net/RDF/DefaultSQLValuesMappingStrategy.cs?at=default
namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public class ValueBinder : IBaseValueBinder
    {
        private ITermMap r2rmlMap;

        private Dictionary<string, ISqlColumn> columns;
        private TemplateProcessor templateProcessor;
        private IEnumerable<ITemplatePart> templateParts;

        private ValueBinder()
        {

        }

        public ValueBinder(ITermMap r2rmlMap, TemplateProcessor templateProcessor)
            : this(null, r2rmlMap, templateProcessor)
        {

        }

        public ValueBinder(string variableName, ITermMap r2rmlMap, TemplateProcessor templateProcessor)
        {
            this.r2rmlMap = r2rmlMap;
            this.columns = new Dictionary<string, ISqlColumn>();
            this.VariableName = variableName;
            this.templateProcessor = templateProcessor;

            if (r2rmlMap.IsConstantValued)
            {
                // No columns needed
            }
            else if (r2rmlMap.IsColumnValued)
            {
                this.columns.Add(r2rmlMap.ColumnName, null);
            }
            else if (r2rmlMap.IsTemplateValued)
            {
                var template = r2rmlMap.Template;

                var columns = templateProcessor.GetColumnsFromTemplate(template);
                templateParts = templateProcessor.ParseTemplate(template);

                foreach (var col in columns)
                {
                    this.columns.Add(col, null);
                }
            }
        }

        public string VariableName { get; private set; }

        public ITermMap R2RMLMap { get { return r2rmlMap; } }

        public TemplateProcessor TemplateProcessor { get { return templateProcessor; } }

        public IEnumerable<string> NeededColumns { get { return columns.Keys.ToArray(); } }

        public void SetColumn(string column, ISqlColumn sqlColumn)
        {
            if (this.columns.ContainsKey(column))
            {
                this.columns[column] = sqlColumn;
            }
            else
            {
                throw new Exception("Cannot set column that is not requested for evaluation");
            }
        }

        public ISqlColumn GetColumn(string column)
        {
            if (this.columns.ContainsKey(column))
            {
                return this.columns[column];
            }
            else
            {
                throw new Exception("Cannot get column that is not requested for evaluation");
            }
        }

        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            var keys = this.columns.Where(x => x.Value == oldColumn).Select(x => x.Key).ToArray();

            foreach (var key in keys)
            {
                SetColumn(key, newColumn);
            }
        }

        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            if (R2RMLMap.IsConstantValued)
            {
                return CreateNodeFromConstant(factory, context);
            }
            else if (R2RMLMap.IsColumnValued)
            {
                return CreateNodeFromColumn(factory, row, context);
            }
            else if (R2RMLMap.IsTemplateValued)
            {
                return CreateNodeFromTemplate(factory, row, context);
            }
            else
            {
                throw new Exception("Term map must be either constant, column or template valued");
            }
        }

        private INode CreateNodeFromTemplate(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            var val = ReplaceColumnReferences(row, context, R2RMLMap.TermType.IsURI);

            if (val != null)
                return GenerateTermForValue(factory, val, context);
            else
                return null;
        }

        private string ReplaceColumnReferences(IQueryResultRow row, QueryContext context, bool escape)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var part in templateParts)
            {
                if (part.IsText)
                    sb.Append(part.Text);
                else if (part.IsColumn)
                {
                    var replaced = ReplaceColumnReference(part.Column, row, context, escape);

                    if (replaced == null)
                        return null;

                    sb.Append(replaced);
                }
            }

            return sb.ToString();
        }

        private string ReplaceColumnReference(string column, IQueryResultRow row, QueryContext context, bool escape)
        {
            var dbCol = row.GetColumn(column);
            var value = dbCol.Value;

            if (value == null)
                return null;

            var sVal = value.ToString();

            return escape ? MappingHelper.UrlEncode(sVal) : sVal;
        }

        private INode CreateNodeFromColumn(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            var column = this.NeededColumns.Select(x => GetColumn(x)).First();

            var dbCol = row.GetColumn(column.Name);

            if (R2RMLMap.TermType.IsLiteral)
            {
                return GenerateTermForLiteral(factory, dbCol.Value, context);
            }
            else
            {
                AssertNoIllegalCharacters(new Uri(dbCol.Value.ToString(), UriKind.RelativeOrAbsolute));
                return GenerateTermForValue(factory, dbCol.Value, context);
            }
        }

        private INode GenerateTermForValue(INodeFactory factory, object value, QueryContext context)
        {
            if (value == null)
                return null;

            var termType = R2RMLMap.TermType;

            if (termType.IsURI)
            {
                try
                {
                    var uri = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);

                    if (!uri.IsAbsoluteUri)
                    {
                        uri = ConstructAbsoluteUri(factory, value.ToString(), context);
                    }

                    if (uri.IsAbsoluteUri)
                    {
                        uri.LeaveDotsAndSlashesEscaped();
                        return factory.CreateUriNode(uri);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Value {0} is invalid uri", value));
                }
            }
            else if (termType.IsBlankNode)
            {
                return GenerateBlankNodeForValue(factory, value, context);
            }
            else if (termType.IsLiteral)
            {
                return GenerateTermForLiteral(factory, value, context);
            }

            throw new Exception(string.Format("Don't know how to generate term for value {0}", value));
        }

        private INode GenerateBlankNodeForValue(INodeFactory factory, object value, QueryContext context)
        {
            if (R2RMLMap is ISubjectMap)
                return context.GetBlankNodeSubjectForValue(factory, value);
            else
                return context.GetBlankNodeObjectForValue(factory, value);
        }

        private Uri ConstructAbsoluteUri(INodeFactory factory, string relativePart, QueryContext context)
        {
            if (relativePart.Split('/').Any(seg => seg == "." || seg == ".."))
                throw new Exception("The relative IRI cannot contain any . or .. parts");

            return new Uri(R2RMLMap.BaseUri + relativePart);
        }

        private INode GenerateTermForLiteral(INodeFactory factory, object value, QueryContext context)
        {
            if (value == null)
                return null;

            if (!(R2RMLMap is ILiteralTermMap))
                throw new Exception("Term map cannot be of term type literal");

            var literalTermMap = R2RMLMap as ILiteralTermMap;
            Uri datatypeUri = literalTermMap.DataTypeURI;
            string language = literalTermMap.Language;

            if (language != null && datatypeUri != null)
                throw new Exception("Literal term map cannot have both language tag and datatype set");

            if (language != null)
                return factory.CreateLiteralNode(value.ToString(), language);
            if (datatypeUri != null)
                return factory.CreateLiteralNode(value.ToString(), datatypeUri);

            return factory.CreateLiteralNode(value.ToString());
        }

        protected INode CreateNodeFromConstant(INodeFactory factory, QueryContext context)
        {
            if (R2RMLMap is IUriValuedTermMap)
            {
                return factory.CreateUriNode(((IUriValuedTermMap)R2RMLMap).URI);
            }
            else if (R2RMLMap is IObjectMap)
            {
                var objectMap = R2RMLMap as IObjectMap;

                if (objectMap.URI != null)
                    return factory.CreateUriNode(objectMap.URI);
                else if (objectMap.Literal != null)
                    return factory.CreateLiteralNode(objectMap.Literal);
                else
                    throw new Exception("Object map's value cannot be both IRI and literal.");
            }

            throw new Exception("Constant must be uri valued or an object map");
        }

        private void AssertNoIllegalCharacters(Uri value)
        {
            IEnumerable<char> disallowedChars = string.Empty;
            IEnumerable<string> segments = value.IsAbsoluteUri ? value.Segments : new[] { value.OriginalString };

            foreach (var segment in segments)
            {
                if (segment.Any(chara => !MappingHelper.IsIUnreserved(chara)))
                {
                    disallowedChars =
                        disallowedChars.Union(
                            segment.Where(chara => chara != '/' && !MappingHelper.IsIUnreserved(chara)));
                }
            }

            var joinedChars = string.Join(",", disallowedChars.Select(c => string.Format("'{0}'", c)));
            if (joinedChars.Any())
            {
                const string format = "Column value is not escaped and thus cannot contain these disallowed characters: {0}";
                var reason = string.Format(format, joinedChars);
                throw new Exception(reason);
            }
        }

        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { return columns.Select(x => x.Value); }
        }

        public object Clone()
        {
            var newBinder = new ValueBinder();
            newBinder.VariableName = this.VariableName;
            newBinder.r2rmlMap = this.r2rmlMap;
            newBinder.templateProcessor = templateProcessor;
            newBinder.templateParts = templateParts;
            newBinder.columns = new Dictionary<string, ISqlColumn>();

            foreach (var item in this.columns)
            {
                newBinder.columns.Add(item.Key, item.Value);
            }

            return newBinder;
        }
    }
}
