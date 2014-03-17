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
using System.Linq.Expressions;
using System.Diagnostics;

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
            this.loadNodeFunc = null;
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
                this.templateParts = templateProcessor.ParseTemplate(template).ToArray();

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
            loadNodeFunc = null;

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
            loadNodeFunc = null;

            var keys = this.columns.Where(x => x.Value == oldColumn).Select(x => x.Key).ToArray();

            foreach (var key in keys)
            {
                SetColumn(key, newColumn);
            }
        }

        private Func<INodeFactory, IQueryResultRow, QueryContext, INode> loadNodeFunc;

        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            if (loadNodeFunc == null)
                loadNodeFunc = GenerateLoadNodeFunc();

            return loadNodeFunc(factory, row, context);
        }

        #region GenerateLoadNodeFunc
        private Func<INodeFactory, IQueryResultRow, QueryContext, INode> GenerateLoadNodeFunc()
        {
            Expression<Func<INodeFactory, IQueryResultRow, QueryContext, INode>> expr = null;

            if (R2RMLMap.IsConstantValued)
            {
                expr = GenerateLoadNodeFuncFromConstant();
            }
            else if (R2RMLMap.IsColumnValued)
            {
                expr = GenerateLoadNodeFuncFromColumn();
            }
            else if (R2RMLMap.IsTemplateValued)
            {
                expr = GenerateLoadNodeFuncFromTemplate();
            }
            else
            {
                throw new Exception("Term map must be either constant, column or template valued");
            }

            return expr.Compile();
        }

        private Expression<Func<INodeFactory, IQueryResultRow, QueryContext, INode>> GenerateLoadNodeFuncFromTemplate()
        {
            ParameterExpression nodeFactory = Expression.Parameter(typeof(INodeFactory), "nodeFactory");
            ParameterExpression row = Expression.Parameter(typeof(IQueryResultRow), "row");
            ParameterExpression context = Expression.Parameter(typeof(QueryContext), "context");

            ParameterExpression valVar = Expression.Parameter(typeof(string), "val");

            List<Expression> expressions = new List<Expression>();
            expressions.Add(Expression.Assign(valVar, GenerateReplaceColumnReferencesFunc(nodeFactory, row, context, R2RMLMap.TermType.IsURI)));
            expressions.Add(Expression.Condition(Expression.Equal(valVar, Expression.Constant(null, typeof(string))),
                Expression.Constant(null, typeof(INode)),
                GenerateTermForValueFunc(nodeFactory, valVar, context))); // Change to generate term for value

            var block = Expression.Block(typeof(INode), new ParameterExpression[] { valVar }, expressions);
            return Expression.Lambda<Func<INodeFactory, IQueryResultRow, QueryContext, INode>>(block, nodeFactory, row, context);
        }

        private Expression GenerateReplaceColumnReferencesFunc(ParameterExpression nodeFactory, ParameterExpression row, ParameterExpression context, bool escape)
        {
            List<Expression> expressions = new List<Expression>();
            ParameterExpression sbVar = Expression.Parameter(typeof(StringBuilder), "sb");
            ParameterExpression replacedVar = Expression.Parameter(typeof(string), "replaced");

            var endLabel = Expression.Label(typeof(string), "returnLabel");

            var appendMethod = typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) });
            expressions.Add(Expression.Assign(sbVar, Expression.New(typeof(StringBuilder))));

            foreach (var part in templateParts)
            {
                if (part.IsText)
                {
                    expressions.Add(Expression.Call(sbVar, appendMethod, Expression.Constant(part.Text, typeof(string))));
                }
                else if (part.IsColumn)
                {
                    expressions.Add(Expression.Assign(replacedVar, GenerateReplaceColumnReferenceFunc(nodeFactory, row, context, part, GetColumn(part.Column), escape)));
                    expressions.Add(Expression.IfThen(Expression.Equal(replacedVar, Expression.Constant(null, typeof(string))), Expression.Return(endLabel, Expression.Constant(null, typeof(string)))));
                    expressions.Add(Expression.Call(sbVar, appendMethod, replacedVar));
                }
            }

            expressions.Add(Expression.Label(endLabel, Expression.Call(sbVar, "ToString", new Type[0])));

            return Expression.Block(typeof(string), new ParameterExpression[] { sbVar, replacedVar }, expressions);
        }

        private Expression GenerateReplaceColumnReferenceFunc(ParameterExpression nodeFactory, ParameterExpression row, ParameterExpression context, ITemplatePart part, ISqlColumn column, bool escape)
        {
            var dbColVar = Expression.Parameter(typeof(IQueryResultColumn), "dbCol");
            var valueVar = Expression.Parameter(typeof(object), "value");
            var sValVar = Expression.Parameter(typeof(string), "sVal");
            var endLabel = Expression.Label(typeof(string), "returnLabel");

            List<Expression> expressions = new List<Expression>();
            expressions.Add(Expression.Assign(dbColVar, Expression.Call(row, "GetColumn", new Type[0], Expression.Constant(column.Name))));
            expressions.Add(Expression.Assign(valueVar, Expression.Property(dbColVar, "Value")));
            expressions.Add(Expression.IfThen(Expression.Equal(valueVar, Expression.Constant(null, typeof(object))), Expression.Return(endLabel, Expression.Constant(null, typeof(string)))));
            expressions.Add(Expression.Assign(sValVar, Expression.Call(valueVar, "ToString", new Type[0])));

            if (escape)
            {
                expressions.Add(Expression.Label(endLabel, Expression.Call(typeof(MappingHelper), "UrlEncode", new Type[0], sValVar)));
            }
            else
            {
                expressions.Add(Expression.Label(endLabel, sValVar));
            }

            return Expression.Block(typeof(string), new ParameterExpression[] { dbColVar, valueVar, sValVar }, expressions);
        }

        private Expression<Func<INodeFactory, IQueryResultRow, QueryContext, INode>> GenerateLoadNodeFuncFromConstant()
        {
            if (R2RMLMap is IUriValuedTermMap)
            {
                var uri = ((IUriValuedTermMap)R2RMLMap).URI;
                return (fact, row, context) => fact.CreateUriNode(uri);
            }
            else if (R2RMLMap is IObjectMap)
            {
                var objectMap = (IObjectMap)R2RMLMap;

                if (objectMap.URI != null)
                    return (fact, row, context) => fact.CreateUriNode(objectMap.URI);
                else if (objectMap.Literal != null)
                    return (fact, row, context) => fact.CreateLiteralNode(objectMap.Literal);
                else
                    throw new Exception("Object map's value must be IRI or literal.");
            }
            else
            {
                throw new Exception("Constant must be uri valued or an object map");
            }
        }

        private Expression GenerateTermForValueFunc(ParameterExpression factory, ParameterExpression value, ParameterExpression context)
        {
            var endLabel = Expression.Label(typeof(INode), "returnLabel");
            List<Expression> expressions = new List<Expression>();
            ParameterExpression nodeVar = Expression.Parameter(typeof(INode), "node");

            expressions.Add(Expression.Assign(nodeVar, Expression.Constant(null, typeof(INode))));

            expressions.Add(Expression.IfThen(Expression.Equal(value, Expression.Constant(null, typeof(object))),
                Expression.Return(endLabel, Expression.Constant(null, typeof(INode)))));

            var termType = R2RMLMap.TermType;

            if (termType.IsURI)
            {
                expressions.Add(Expression.Assign(nodeVar,
                    Expression.Call(typeof(ValueBinder), "GenerateUriTermForValue", new Type[0],
                        Expression.Call(value, "ToString", new Type[0]),
                        factory,
                        context,
                        Expression.Constant(R2RMLMap.BaseUri, typeof(Uri)))));
            }
            else if (termType.IsBlankNode)
            {
                expressions.Add(Expression.Assign(nodeVar, GenerateBlankNodeForValueFunc(factory, value, context)));
            }
            else if (termType.IsLiteral)
            {
                expressions.Add(Expression.Assign(nodeVar, GenerateTermForLiteralFunc(factory, value, context)));
            }
            else
            {
                throw new Exception(string.Format("Unhandled term type", value));
            }

            expressions.Add(Expression.Label(endLabel, nodeVar));

            return Expression.Block(typeof(INode), new ParameterExpression[] { nodeVar }, expressions);
        }

        private Expression GenerateBlankNodeForValueFunc(ParameterExpression factory, ParameterExpression value, ParameterExpression context)
        {
            if (R2RMLMap is ISubjectMap)
                return Expression.Call(context, "GetBlankNodeSubjectForValue", new Type[0], factory, value);
            else
                return Expression.Call(context, "GetBlankNodeObjectForValue", new Type[0], factory, value);
        }

        private Expression GenerateTermForLiteralFunc(ParameterExpression factory, ParameterExpression value, ParameterExpression context)
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
                return Expression.Call(factory, "CreateLiteralNode", new Type[0], Expression.Call(value, "ToString", new Type[0]), Expression.Constant(language, typeof(string)));
            if (datatypeUri != null)
                return Expression.Call(factory, "CreateLiteralNode", new Type[0], Expression.Call(value, "ToString", new Type[0]), Expression.Constant(datatypeUri, typeof(Uri)));

            return Expression.Call(factory, "CreateLiteralNode", new Type[0], Expression.Call(value, "ToString", new Type[0]));
        }

        private static INode GenerateUriTermForValue(string value, INodeFactory factory, QueryContext context, Uri baseUri)
        {
            try
            {
                var uri = new Uri(value, UriKind.RelativeOrAbsolute);

                if (!uri.IsAbsoluteUri)
                {
                    uri = ConstructAbsoluteUri(factory, value, baseUri, context);
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
                throw new Exception(string.Format("Value {0} is invalid uri", value));
            }
        }

        private static Uri ConstructAbsoluteUri(INodeFactory factory, string relativePart, Uri baseUri, QueryContext context)
        {
            if (relativePart.Split('/').Any(seg => seg == "." || seg == ".."))
                throw new Exception("The relative IRI cannot contain any . or .. parts");

            return new Uri(baseUri + relativePart);
        }

        private static void AssertNoIllegalCharacters(Uri value)
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

        private Expression<Func<INodeFactory, IQueryResultRow, QueryContext, INode>> GenerateLoadNodeFuncFromColumn()
        {
            ParameterExpression nodeFactory = Expression.Parameter(typeof(INodeFactory), "nodeFactory");
            ParameterExpression row = Expression.Parameter(typeof(IQueryResultRow), "row");
            ParameterExpression context = Expression.Parameter(typeof(QueryContext), "context");

            var column = this.NeededColumns.Select(x => GetColumn(x)).First();
            ParameterExpression dbColVar = Expression.Parameter(typeof(IQueryResultColumn), "dbCol");
            ParameterExpression valVar = Expression.Parameter(typeof(object), "value");

            List<Expression> expressions = new List<Expression>();
            expressions.Add(Expression.Assign(dbColVar, Expression.Call(row, "GetColumn", new Type[0], Expression.Constant(column.Name, typeof(string)))));
            expressions.Add(Expression.Assign(valVar, Expression.Property(dbColVar, "Value")));

            if (R2RMLMap.TermType.IsLiteral)
            {
                expressions.Add(GenerateTermForLiteralFunc(nodeFactory, valVar, context));
            }
            else
            {
                expressions.Add(Expression.Call(typeof(ValueBinder), "AssertNoIllegalCharacters", new Type[0],
                    Expression.New(typeof(Uri).GetConstructor(new Type[] { typeof(string), typeof(UriKind) }),
                        Expression.Call(valVar, "ToString", new Type[0]),
                        Expression.Constant(UriKind.RelativeOrAbsolute, typeof(UriKind)))));
                expressions.Add(GenerateTermForValueFunc(nodeFactory, valVar, context));
            }

            var block = Expression.Block(typeof(INode), new ParameterExpression[] { dbColVar, valVar }, expressions);
            return Expression.Lambda<Func<INodeFactory, IQueryResultRow, QueryContext, INode>>(block, nodeFactory, row, context);
        } 
        #endregion

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

        [DebuggerStepThrough]
        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
