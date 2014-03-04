using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Binders;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql
{
    public class ExpressionBuilder
    {
        public IExpression CreateExpression(QueryContext context, INode node)
        {
            if (node is UriNode)
            {
                return CreateExpression(context, (UriNode)node);
            }

            // TODO: Other INode types
            // http://dotnetrdf.org/API/dotNetRDF~VDS.RDF.INode.html
            //BlankNode
            //GraphLiteralNode
            //LiteralNode
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

        private IExpression CreateExpression(QueryContext context, UriNode node)
        {
            return new ConstantExpr(node.Uri);
        }

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
                    // TODO: Handle object map
                    throw new NotImplementedException();
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

        public IExpression CreateColumnExpression(QueryContext context, ISqlColumn sqlColumn, bool isIriEscaped)
        {
            return new ColumnExpr(sqlColumn, isIriEscaped);
        }
    }
}
