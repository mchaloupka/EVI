using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Binders.Utils;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Slp.r2rml4net.Storage.Sql
{
    /// <summary>
    /// Builder for SQL expression
    /// </summary>
    public class ExpressionBuilder
    {
        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="node">The node.</param>
        /// <returns>The created expression.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IExpression CreateExpression(QueryContext context, INode node)
        {
            if (node is UriNode)
            {
                return CreateExpression(context, (UriNode)node);
            }
            else if (node is LiteralNode)
            {
                return CreateExpression(context, (LiteralNode)node);
            }

            // TODO: Other INode types
            // http://dotnetrdf.org/API/dotNetRDF~VDS.RDF.INode.html
            //BlankNode
            //GraphLiteralNode
            //BooleanNode
            //ByteNode
            //DateNode
            //DateTimeNode
            //DecimalNode
            //DoubleNode
            //FloatNode
            //LongNode
            //NumericNode
            //SignedByteNode
            //StringNode
            //TimeSpanNode
            //UnsignedLongNode

            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="node">The URI node.</param>
        /// <returns>The created expression.</returns>
        private IExpression CreateExpression(QueryContext context, UriNode node)
        {
            return new ConstantExpr(node.Uri);
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="node">The literal node.</param>
        /// <returns>The created expression.</returns>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        private IExpression CreateExpression(QueryContext context, LiteralNode node)
        {
            if(node.DataType == null)
            {
                return new ConstantExpr(node.Value);
            }

            switch (node.DataType.AbsoluteUri)
            {
                case XmlSpecsHelper.XmlSchemaDataTypeInt:
                case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                    return new ConstantExpr(int.Parse(node.Value));
                default:
                    throw new NotImplementedException();
            }

            // TODO: https://bitbucket.org/dotnetrdf/dotnetrdf/src/b37d1707735f727613d0804a7a81a56b2a7e6ce3/Libraries/core/net40/Parsing/XMLSpecsHelper.cs?at=default
            //XmlSchemaDataTypeAnyUri = NamespaceXmlSchema + "anyURI",
            //XmlSchemaDataTypeBase64Binary = NamespaceXmlSchema + "base64Binary",
            //XmlSchemaDataTypeBoolean = NamespaceXmlSchema + "boolean",
            //XmlSchemaDataTypeByte = NamespaceXmlSchema + "byte",
            //XmlSchemaDataTypeDate = NamespaceXmlSchema + "date",
            //XmlSchemaDataTypeDateTime = NamespaceXmlSchema + "dateTime",
            //XmlSchemaDataTypeDayTimeDuration = NamespaceXmlSchema + "dayTimeDuration",
            //XmlSchemaDataTypeDuration = NamespaceXmlSchema + "duration",
            //XmlSchemaDataTypeDecimal = NamespaceXmlSchema + "decimal",
            //XmlSchemaDataTypeDouble = NamespaceXmlSchema + "double",
            //XmlSchemaDataTypeFloat = NamespaceXmlSchema + "float",
            //XmlSchemaDataTypeHexBinary = NamespaceXmlSchema + "hexBinary",
            //XmlSchemaDataTypeInt = NamespaceXmlSchema + "int",
            //XmlSchemaDataTypeInteger = NamespaceXmlSchema + "integer",
            //XmlSchemaDataTypeLong = NamespaceXmlSchema + "long",
            //XmlSchemaDataTypeNegativeInteger = NamespaceXmlSchema + "negativeInteger",
            //XmlSchemaDataTypeNonNegativeInteger = NamespaceXmlSchema + "nonNegativeInteger",
            //XmlSchemaDataTypeNonPositiveInteger = NamespaceXmlSchema + "nonPositiveInteger",
            //XmlSchemaDataTypePositiveInteger = NamespaceXmlSchema + "positiveInteger",
            //XmlSchemaDataTypeShort = NamespaceXmlSchema + "short",
            //XmlSchemaDataTypeTime = NamespaceXmlSchema + "time",
            //XmlSchemaDataTypeString = NamespaceXmlSchema + "string",
            //XmlSchemaDataTypeUnsignedByte = NamespaceXmlSchema + "unsignedByte",
            //XmlSchemaDataTypeUnsignedInt = NamespaceXmlSchema + "unsignedInt",
            //XmlSchemaDataTypeUnsignedLong = NamespaceXmlSchema + "unsignedLong",
            //XmlSchemaDataTypeUnsignedShort = NamespaceXmlSchema + "unsignedShort";
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="valueBinder">The value binder.</param>
        /// <returns>The created expression.</returns>
        /// <exception cref="System.Exception">
        /// Object map's value must be IRI or literal.
        /// or
        /// Unknonwn constant valued term map
        /// or
        /// Template part must be column or text part
        /// or
        /// Mapping can be only constant, column or template valued
        /// </exception>
        public IExpression CreateExpression(QueryContext context, ValueBinder valueBinder)
        {
            var map = valueBinder.R2RMLMap;

            if (map.IsConstantValued)
            {
                if (map is IUriValuedTermMap)
                {
                    return new ConstantExpr(((IUriValuedTermMap)map).URI);
                }
                else if (map is IObjectMap)
                {
                    var objectMap = (IObjectMap)map;

                    if (objectMap.URI != null)
                        return new ConstantExpr(objectMap.URI);
                    else if (objectMap.Literal != null)
                    {
                        // TODO: Rework - better node creation

                        if (objectMap.Literal.Contains("^^"))
                        {
                            var split = objectMap.Literal.Split(new string[] { "^^" }, 2, StringSplitOptions.None);
                            var node = context.NodeFactory.CreateLiteralNode(split[0], UriFactory.Create(split[1]));
                            return CreateExpression(context, node);
                        }
                        else
                        {
                            var node = context.NodeFactory.CreateLiteralNode(objectMap.Literal);
                            return CreateExpression(context, node);
                        }
                    }
                    else
                        throw new Exception("Object map's value must be IRI or literal.");
                }
                else
                {
                    throw new Exception("Unknonwn constant valued term map");
                }
            }
            else if (map.IsColumnValued)
            {
                return CreateColumnExpression(context, valueBinder.GetColumn(map.ColumnName), false);
            }
            else if (map.IsTemplateValued)
            {
                var parsedTemplate = valueBinder.TemplateProcessor.ParseTemplate(map.Template);

                List<IExpression> parts = new List<IExpression>();

                foreach (var templatePart in parsedTemplate)
                {
                    if (templatePart.IsColumn)
                    {
                        parts.Add(CreateColumnExpression(context, valueBinder.GetColumn(templatePart.Column), map.TermType.IsURI));
                    }
                    else if (templatePart.IsText)
                    {
                        parts.Add(new ConstantExpr(templatePart.Text));
                    }
                    else
                    {
                        throw new Exception("Template part must be column or text part");
                    }
                }

                if (parts.Count == 0)
                {
                    // TODO: Handle empty template (or should I not?)
                    return new ConstantExpr(string.Empty);
                }
                else if (parts.Count == 1)
                {
                    return parts[0];
                }
                else
                {
                    return new ConcatenationExpr(parts);
                }
            }
            else
            {
                throw new Exception("Mapping can be only constant, column or template valued");
            }
        }

        /// <summary>
        /// Creates the column expression.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="sqlColumn">The SQL column.</param>
        /// <param name="isIriEscaped">if set to <c>true</c> the value should be iri escaped.</param>
        /// <returns>The created expression.</returns>
        public IExpression CreateColumnExpression(QueryContext context, ISqlColumn sqlColumn, bool isIriEscaped)
        {
            return new ColumnExpr(sqlColumn, isIriEscaped);
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="number">The number.</param>
        /// <returns>The created expression.</returns>
        public IExpression CreateExpression(QueryContext context, int number)
        {
            return new ConstantExpr(number);
        }

        /// <summary>
        /// Creates the order by expression.
        /// </summary>
        /// <param name="sparqlQueryExpression">The sparql query expression.</param>
        /// <param name="select">The select operator.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The created expression.</returns>
        public IExpression CreateOrderByExpression(ISparqlQueryExpression sparqlQueryExpression, SqlSelectOp select, QueryContext context)
        {
            // TODO: Handle order specifics
            // http://www.w3.org/TR/2013/REC-sparql11-query-20130321/#modOrderBy

            return ConvertExpression(sparqlQueryExpression, select.ValueBinders.Select(x => x.GetOriginalValueBinder(context)).ToList(), context);
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <param name="binder">The value binder.</param>
        /// <returns>The created expression.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IExpression CreateExpression(QueryContext context, IBaseValueBinder binder)
        {
            if (binder is ValueBinder)
            {
                return CreateExpression(context, (ValueBinder)binder);
            }
            else if (binder is CoalesceValueBinder)
            {
                var col = new CoalesceExpr();

                foreach (var innerBinder in ((CoalesceValueBinder)binder).InnerBinders)
                {
                    col.AddExpression(CreateExpression(context, innerBinder));
                }

                return col;
            }
            else if (binder is CaseValueBinder)
            {
                var cas = new CaseExpr();

                foreach (var statement in ((CaseValueBinder)binder).Statements)
                {
                    cas.AddStatement(statement.Condition, CreateExpression(context, statement.ValueBinder));
                }

                return cas;
            }
            else if(binder is BlankValueBinder)
            {
                return new NullExpr();
            }
            else if (binder is SqlSideValueBinder)
            {
                var sqlSide = (SqlSideValueBinder)binder;
                var column = sqlSide.Column;
                return CreateColumnExpression(context, column, false);
            }
            else if (binder is ExpressionValueBinder)
            {
                return (IExpression)((ExpressionValueBinder)binder).Expression.Clone();
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the expression for SQL side value binder.
        /// </summary>
        /// <param name="binder">The value binder.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The created expression.</returns>
        public IExpression CreateExpressionForSqlSideValueBinder(IBaseValueBinder binder, QueryContext context)
        {
            return CreateExpression(context, binder.GetOriginalValueBinder(context));
        }

        /// <summary>
        /// Converts the expression.
        /// </summary>
        /// <param name="sparqlQueryExpression">The sparql query expression.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The created expression.</returns>
        /// <exception cref="System.Exception">Variable not found</exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public IExpression ConvertExpression(ISparqlQueryExpression sparqlQueryExpression, ICollection<IBaseValueBinder> valueBinders, QueryContext context)
        {
            if (sparqlQueryExpression is VariableT)
            {
                var varExpr = (VariableT)sparqlQueryExpression;
                var valueBinder = valueBinders.FirstOrDefault(x => x.VariableName == varExpr.Variable);

                if (valueBinder == null)
                    throw new Exception("Variable not found");

                return CreateExpression(context, valueBinder);
            }
            else if (sparqlQueryExpression is ConcatF)
            {
                var concExpr = (ConcatF)sparqlQueryExpression;
                var parts = concExpr.Parts.Select(x => ConvertExpression(x, valueBinders, context));

                return new ConcatenationExpr(parts);
            }
            else if (sparqlQueryExpression is ConstantT)
            {
                var constT = (ConstantT)sparqlQueryExpression;
                var node = constT.Node;
                return ConvertExpression(node, context);
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the expression.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The created expression.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private IExpression ConvertExpression(IValuedNode node, QueryContext context)
        {
            if (node is StringNode)
            {
                var sn = (StringNode)node;
                return new ConstantExpr(sn.Value);
            }
            else
                throw new NotImplementedException();

            // TODO: Implement the rest
            // http://www.dotnetrdf.org/api/dotNetRDF~VDS.RDF.Nodes.IValuedNode.html
        }
    }
}
