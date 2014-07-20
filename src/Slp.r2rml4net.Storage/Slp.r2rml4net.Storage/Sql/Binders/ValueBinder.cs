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
    /// <summary>
    /// The basic value binder
    /// </summary>
    public class ValueBinder : IBaseValueBinder
    {
        /// <summary>
        /// The R2RML map
        /// </summary>
        private ITermMap r2rmlMap;

        /// <summary>
        /// The columns
        /// </summary>
        private Dictionary<string, ISqlColumn> columns;

        /// <summary>
        /// The template processor
        /// </summary>
        private TemplateProcessor templateProcessor;

        /// <summary>
        /// The template parts
        /// </summary>
        private IEnumerable<ITemplatePart> templateParts;

        /// <summary>
        /// Prevents a default instance of the <see cref="ValueBinder"/> class from being publicly created.
        /// </summary>
        private ValueBinder()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBinder"/> class.
        /// </summary>
        /// <param name="r2rmlMap">The R2RML map.</param>
        /// <param name="templateProcessor">The template processor.</param>
        public ValueBinder(ITermMap r2rmlMap, TemplateProcessor templateProcessor)
            : this(null, r2rmlMap, templateProcessor)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="r2rmlMap">The R2RML map.</param>
        /// <param name="templateProcessor">The template processor.</param>
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

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; private set; }

        /// <summary>
        /// Gets the R2RML map.
        /// </summary>
        /// <value>The R2RML map.</value>
        public ITermMap R2RMLMap { get { return r2rmlMap; } }

        /// <summary>
        /// Gets the template processor.
        /// </summary>
        /// <value>The template processor.</value>
        public TemplateProcessor TemplateProcessor { get { return templateProcessor; } }

        /// <summary>
        /// Gets the needed columns.
        /// </summary>
        /// <value>The needed columns.</value>
        public IEnumerable<string> NeededColumns { get { return columns.Keys.ToArray(); } }

        /// <summary>
        /// Sets the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="sqlColumn">The SQL column.</param>
        /// <exception cref="System.Exception">Cannot set column that is not requested for evaluation</exception>
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

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>ISqlColumn.</returns>
        /// <exception cref="System.Exception">Cannot get column that is not requested for evaluation</exception>
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

        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            loadNodeFunc = null;

            var keys = this.columns.Where(x => x.Value == oldColumn).Select(x => x.Key).ToArray();

            foreach (var key in keys)
            {
                SetColumn(key, newColumn);
            }
        }

        /// <summary>
        /// The load node function
        /// </summary>
        private Func<INodeFactory, IQueryResultRow, QueryContext, INode> loadNodeFunc;

        /// <summary>
        /// Loads the node value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="row">The db row.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The node.</returns>
        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            if (loadNodeFunc == null)
                loadNodeFunc = GenerateLoadNodeFunc();

            return loadNodeFunc(factory, row, context);
        }

        #region GenerateLoadNodeFunc
        /// <summary>
        /// Generates the load node function.
        /// </summary>
        /// <returns>Generated function.</returns>
        /// <exception cref="System.Exception">Term map must be either constant, column or template valued</exception>
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

        /// <summary>
        /// Generates the load node function from template.
        /// </summary>
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

        /// <summary>
        /// Generates the replace column references function.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="row">The row.</param>
        /// <param name="context">The context.</param>
        /// <param name="escape">if set to <c>true</c> the value should be escaped.</param>
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

        /// <summary>
        /// Generates the replace column reference function.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="row">The row.</param>
        /// <param name="context">The context.</param>
        /// <param name="part">The part.</param>
        /// <param name="column">The column.</param>
        /// <param name="escape">if set to <c>true</c> the value should be escaped.</param>
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

        /// <summary>
        /// Generates the load node function from constant.
        /// </summary>
        /// <exception cref="System.Exception">
        /// Object map's value must be IRI or literal.
        /// or
        /// Constant must be uri valued or an object map
        /// </exception>
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

        /// <summary>
        /// Generates the term for value function.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The query context.</param>
        /// <exception cref="System.Exception"></exception>
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

        /// <summary>
        /// Generates the blank node for value function.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The query context.</param>
        private Expression GenerateBlankNodeForValueFunc(ParameterExpression factory, ParameterExpression value, ParameterExpression context)
        {
            if (R2RMLMap is ISubjectMap)
                return Expression.Call(context, "GetBlankNodeSubjectForValue", new Type[0], factory, value);
            else
                return Expression.Call(context, "GetBlankNodeObjectForValue", new Type[0], factory, value);
        }

        /// <summary>
        /// Generates the term for literal function.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The query context.</param>
        /// <exception cref="System.Exception">
        /// Term map cannot be of term type literal
        /// or
        /// Literal term map cannot have both language tag and datatype set
        /// </exception>
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

        /// <summary>
        /// Generates the URI term for value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <exception cref="System.Exception">
        /// Now the uri must be absolute
        /// or
        /// </exception>
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

        /// <summary>
        /// Constructs the absolute URI.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="relativePart">The relative part.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="context">The context.</param>
        /// <returns>Uri.</returns>
        /// <exception cref="System.Exception">The relative IRI cannot contain any . or .. parts</exception>
        private static Uri ConstructAbsoluteUri(INodeFactory factory, string relativePart, Uri baseUri, QueryContext context)
        {
            if (relativePart.Split('/').Any(seg => seg == "." || seg == ".."))
                throw new Exception("The relative IRI cannot contain any . or .. parts");

            return new Uri(baseUri + relativePart);
        }

        /// <summary>
        /// Asserts the no illegal characters.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception"></exception>
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

        /// <summary>
        /// Generates the load node function from column.
        /// </summary>
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

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get { return columns.Select(x => x.Value); }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
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
