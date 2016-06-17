using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Sparql.Algebra;
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
        private readonly ValueBinder_CreateIsBoundCondition _valueBinderCreateIsBoundCondition;
        private readonly ValueBinder_CreateExpression _valueBinderCreateExpression;
        private readonly SparqlExpression_CreateExpression _sparqlExpressionCreateExpression;
        private readonly Expression_IsBoundCondition _expressionIsBoundCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionBuilder"/> class.
        /// </summary>
        public ConditionBuilder()
        {
            _valueBinderCreateIsBoundCondition = new ValueBinder_CreateIsBoundCondition(this);
            _valueBinderCreateExpression = new ValueBinder_CreateExpression(this);
            _sparqlExpressionCreateExpression = new SparqlExpression_CreateExpression(this);
            _expressionIsBoundCondition = new Expression_IsBoundCondition(this);
        }

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

                return new ComparisonCondition(leftOperand, rightOperand, ComparisonTypes.EqualTo);
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
            return _valueBinderCreateIsBoundCondition.CreateIsBoundCondition(valueBinder, context);
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
                return new ComparisonCondition(leftOperand, rightOperand, ComparisonTypes.EqualTo);
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
                    new ComparisonCondition(new ColumnExpression(context, switchValueBinder.CaseVariable, false), new ConstantExpression(curCase.CaseValue, context), ComparisonTypes.EqualTo),
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
            return new DisjunctionCondition(new IFilterCondition[]
            {
                new NegationCondition(CreateIsBoundCondition(valueBinder, context)),
                new NegationCondition(CreateIsBoundCondition(otherValueBinder, context)),
                CreateEqualsCondition(valueBinder, otherValueBinder, context)
            });
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="valueBinder">The value binder.</param>
        public IExpression CreateExpression(QueryContext context, IValueBinder valueBinder)
        {
            return _valueBinderCreateExpression.CreateExpression(context, valueBinder);
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

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="context">The query context.</param>
        /// <param name="valueBinders">The used value binders.</param>
        public IFilterCondition CreateCondition(ISparqlCondition condition, QueryContext context, IEnumerable<IValueBinder> valueBinders)
        {
            return _sparqlExpressionCreateExpression.CreateCondition(condition, context, valueBinders);
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <returns>IExpression.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IExpression CreateExpression(QueryContext context, ISparqlExpression expression, List<IValueBinder> valueBinders)
        {
            return _sparqlExpressionCreateExpression.CreateExpression(expression, context, valueBinders);
        }

        /// <summary>
        /// Creates the is bound condition.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateIsBoundCondition(IExpression expression, QueryContext context)
        {
            return _expressionIsBoundCondition.CreateIsBoundCondition(expression, context);
        }
    }
}
