using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Slp.Evi.Storage.Relational.Builder
{
    /// <summary>
    /// The expression builder
    /// </summary>
    public class ExpressionBuilder
    {
        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="valueBinder">The value binder.</param>
        public IExpression CreateExpression(QueryContext context, IValueBinder valueBinder)
        {
            if (valueBinder is EmptyValueBinder)
            {
                throw new ArgumentException();
            }
            else if (valueBinder is BaseValueBinder)
            {
                return CreateBaseValueBinderExpression(context, (BaseValueBinder)valueBinder);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates the base value binder expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="valueBinder">The value binder.</param>
        /// <returns>IExpression.</returns>
        private IExpression CreateBaseValueBinderExpression(QueryContext context, BaseValueBinder valueBinder)
        {
            var map = valueBinder.TermMap;

            if (map.IsConstantValued)
            {
                if (map is IUriValuedTermMap)
                {
                    return new ConstantExpression(((IUriValuedTermMap)map).URI, context);
                }
                else if (map is IObjectMap)
                {
                    var objectMap = (IObjectMap) map;

                    if (objectMap.URI != null)
                    {
                        return new ConstantExpression(objectMap.URI, context);
                    }
                    else if (objectMap.Literal != null)
                    {
                        // TODO: Rework - better node creation - ideally implemented in R2RML4NET

                        if (objectMap.Literal.Contains("^^"))
                        {
                            var split = objectMap.Literal.Split(new[] { "^^" }, 2, StringSplitOptions.None);
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
                    {
                        throw new Exception("Object map's value must be IRI or literal.");
                    }
                }
                else
                {
                    throw new Exception("Unknonwn constant valued term map");
                }
            }
            else if (map.IsColumnValued)
            {
                return new ColumnExpression(context, valueBinder.GetCalculusVariable(map.ColumnName), map.TermType.IsURI);
            }
            else if (map.IsTemplateValued)
            {
                List<IExpression> parts = new List<IExpression>();

                foreach (var templatePart in valueBinder.TemplateParts)
                {
                    if (templatePart.IsColumn)
                    {
                        parts.Add(new ColumnExpression(context, valueBinder.GetCalculusVariable(templatePart.Column),
                            map.TermType.IsURI));
                    }
                    else if (templatePart.IsText)
                    {
                        parts.Add(new ConstantExpression(templatePart.Text, context));
                    }
                    else
                    {
                        throw new Exception("Must be column or constant");
                    }
                }

                if (parts.Count == 0)
                {
                    return new ConstantExpression(string.Empty, context);
                }
                else if (parts.Count == 1)
                {
                    return parts[0];
                }
                else
                {
                    return new ConcatenationExpression(parts, context.Db.SqlTypeForString);
                }
            }
            else
            {
                throw new Exception("Mapping can be only constant, column or template valued");
            }
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="node">The node.</param>
        public IExpression CreateExpression(QueryContext context, INode node)
        {
            if (node is UriNode)
            {
                return new ConstantExpression(((UriNode)node).Uri, context);
            }
            else if (node is LiteralNode)
            {
                return CreateLiteralExpression(context, (LiteralNode) node);
            }
            else
            {
                throw new NotImplementedException();

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
            }
        }

        /// <summary>
        /// Creates the literal expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="node">The node.</param>
        private IExpression CreateLiteralExpression(QueryContext context, LiteralNode node)
        {
            if (node.DataType == null)
            {
                return new ConstantExpression(node.Value, context);
            }
            else
            {
                switch (node.DataType.AbsoluteUri)
                {
                    case XmlSpecsHelper.XmlSchemaDataTypeInt:
                    case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                        return new ConstantExpression(int.Parse(node.Value), context);
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

        }
    }
}
