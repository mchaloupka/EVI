using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Slp.Evi.Storage.Relational.Builder
{
    /// <summary>
    /// The conditions builder
    /// </summary>
    public class ConditionBuilder
    {
        /// <summary>
        /// Creates the equals conditions.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="valueBinder">The value binder.</param>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable&lt;ICondition&gt;.</returns>
        public IFilterCondition CreateEqualsCondition(INode node, IValueBinder valueBinder, QueryContext context)
        {
            if (valueBinder is EmptyValueBinder)
            {
                return new AlwaysFalseCondition();
            }
            else if (valueBinder is BaseValueBinder)
            {
                var leftOperand = CreateExpression(context, valueBinder);
                var rightOperand = CreateExpression(context, node);

                return new EqualExpressionCondition(leftOperand, rightOperand);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates the is not null conditions.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateIsBoundCondition(IValueBinder valueBinder, QueryContext context)
        {
            if (valueBinder is EmptyValueBinder)
            {
                return new AlwaysFalseCondition();
            }
            else if (valueBinder is BaseValueBinder)
            {
                var needed = valueBinder.NeededCalculusVariables;

                if (needed.Count() > 0)
                {
                    List<IFilterCondition> conditions = new List<IFilterCondition>();

                    foreach (var calculusVariable in needed)
                    {
                        conditions.Add(new NegationCondition(new IsNullCondition(calculusVariable)));
                    }

                    return new ConjunctionCondition(conditions);
                }
                else
                {
                    return new AlwaysTrueCondition();
                }
            }
            else if (valueBinder is CoalesceValueBinder)
            {
                var coalesceValueBinder = (CoalesceValueBinder) valueBinder;

                return new DisjunctionCondition(
                    coalesceValueBinder.ValueBinders.Select(x => CreateIsBoundCondition(x, context)).ToList());
            }
            else if (valueBinder is SwitchValueBinder)
            {
                var switchValueBinder = (SwitchValueBinder) valueBinder;

                return new DisjunctionCondition(switchValueBinder.Cases.Select(x => new ConjunctionCondition(new IFilterCondition[]
                {
                    new EqualExpressionCondition(new ColumnExpression(context, switchValueBinder.CaseVariable, false), new ConstantExpression(x.CaseValue, context)),
                    CreateIsBoundCondition(x.ValueBinder, context)
                })));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates the disjunction of the conditions
        /// </summary>
        /// <param name="context">The query context</param>
        /// <param name="conditions">The conditions, conjunction of every array member taken as the disjunction parameter</param>
        /// <returns></returns>
        public IFilterCondition CreateDisjunctionCondition(QueryContext context,
            params IFilterCondition[] conditions)
        {
            return new DisjunctionCondition(conditions);
        }

        /// <summary>
        /// Creates the conjunction of the conditions.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        /// <param name="context">The query context.</param>
        /// <returns></returns>
        public IFilterCondition CreateConjunctionCondition(IEnumerable<IFilterCondition> conditions,
            QueryContext context)
        {
            return new ConjunctionCondition(conditions);
        }

        /// <summary>
        /// Create negation conditions
        /// </summary>
        /// <param name="condition">The conditions</param>
        /// <param name="context">The query context.</param>
        /// <returns>The negation of the conditions.</returns>
        public IFilterCondition CreateNegationCondition(IFilterCondition condition,
            QueryContext context)
        {
            return new NegationCondition(condition);
        }

        /// <summary>
        /// Creates the equals conditions.
        /// </summary>
        /// <param name="firstValueBinder">The first value binder.</param>
        /// <param name="secondValueBinder">The second value binder.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateEqualsCondition(IValueBinder firstValueBinder, IValueBinder secondValueBinder, QueryContext context)
        {
            if (firstValueBinder is EmptyValueBinder)
            {
                return new NegationCondition(CreateIsBoundCondition(secondValueBinder, context));
            }
            else if (firstValueBinder is BaseValueBinder && secondValueBinder is BaseValueBinder)
            {
                var leftOperand = CreateExpression(context, firstValueBinder);
                var rightOperand = CreateExpression(context, secondValueBinder);
                return new EqualExpressionCondition(leftOperand, rightOperand);
            }
            else if (firstValueBinder is CoalesceValueBinder)
            {
                var disjunctionConditions = new List<IFilterCondition>();
                var binders = ((CoalesceValueBinder) firstValueBinder).ValueBinders.ToArray();

                for (int curIndex = 0; curIndex < binders.Length; curIndex++)
                {
                    var conjunctionConditions = new List<IFilterCondition>();

                    for (int prevIndex = 0; prevIndex < curIndex; prevIndex++)
                    {
                        conjunctionConditions.Add(new NegationCondition(CreateIsBoundCondition(binders[prevIndex], context)));
                    }

                    conjunctionConditions.Add(CreateEqualsCondition(binders[curIndex], secondValueBinder, context));
                    disjunctionConditions.Add(new DisjunctionCondition(conjunctionConditions));
                }

                return new DisjunctionCondition(disjunctionConditions);
            }
            else if (secondValueBinder is CoalesceValueBinder)
            {
                return CreateEqualsCondition(secondValueBinder, firstValueBinder, context);
            }
            else if (firstValueBinder is SwitchValueBinder)
            {
                var switchValueBinder = (SwitchValueBinder) firstValueBinder;

                return new DisjunctionCondition(switchValueBinder.Cases.Select(curCase => new ConjunctionCondition(new IFilterCondition[]
                {
                    new EqualExpressionCondition(new ColumnExpression(context, switchValueBinder.CaseVariable, false), new ConstantExpression(curCase.CaseValue, context)),
                    CreateEqualsCondition(curCase.ValueBinder, secondValueBinder, context)
                })).ToList());
            }
            else if (secondValueBinder is SwitchValueBinder)
            {
                return CreateEqualsCondition(secondValueBinder, firstValueBinder, context);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates the join equal condition
        /// </summary>
        /// <param name="valueBinder">First value binder</param>
        /// <param name="otherValueBinder">Other value binder</param>
        /// <param name="context">The query context</param>
        public IFilterCondition CreateJoinEqualCondition(IValueBinder valueBinder, IValueBinder otherValueBinder, QueryContext context)
        {
            return CreateDisjunctionCondition(context,
                CreateNegationCondition(CreateIsBoundCondition(valueBinder, context), context),
                CreateNegationCondition(CreateIsBoundCondition(otherValueBinder, context), context),
                CreateEqualsCondition(valueBinder, otherValueBinder, context));
        }

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
            else if (valueBinder is CoalesceValueBinder)
            {
                var coalesceValueBinder = (CoalesceValueBinder)valueBinder;
                List<IExpression> expressions = new List<IExpression>();

                foreach (var binder in coalesceValueBinder.ValueBinders)
                {
                    expressions.Add(CreateExpression(context, binder));
                }

                return new CoalesceExpression(expressions);
            }
            else if (valueBinder is SwitchValueBinder)
            {
                var switchValueBinder = (SwitchValueBinder)valueBinder;
                var statements = new List<CaseExpression.Statement>();

                foreach (var @case in switchValueBinder.Cases)
                {
                    var expression = CreateExpression(context, @case.ValueBinder);
                    var condition = new EqualExpressionCondition(new ColumnExpression(context, switchValueBinder.CaseVariable, false), new ConstantExpression(@case.CaseValue, context));
                    statements.Add(new CaseExpression.Statement(condition, expression));
                }

                return new CaseExpression(statements);
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
                    var objectMap = (IObjectMap)map;

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
                return CreateLiteralExpression(context, (LiteralNode)node);
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
