using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Sparql.Algebra;
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
        public IFilterCondition CreateEqualsCondition(INode node, IValueBinder valueBinder, IQueryContext context)
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
        public IFilterCondition CreateIsBoundCondition(IValueBinder valueBinder, IQueryContext context)
        {
            return _valueBinderCreateIsBoundCondition.CreateIsBoundCondition(valueBinder, context);
        }

        /// <summary>
        /// Creates the equals conditions.
        /// </summary>
        /// <param name="firstValueBinder">The first value binder.</param>
        /// <param name="secondValueBinder">The second value binder.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateEqualsCondition(IValueBinder firstValueBinder, IValueBinder secondValueBinder, IQueryContext context)
        {
            if (firstValueBinder is EmptyValueBinder)
            {
                return new NegationCondition(CreateIsBoundCondition(secondValueBinder, context));
            }
            else if (firstValueBinder is BaseValueBinder leftValueBinder && secondValueBinder is BaseValueBinder rightValueBinder)
            {
                var leftOperand = CreateExpression(context, firstValueBinder);
                var rightOperand = CreateExpression(context, secondValueBinder);

                var leftType = leftValueBinder.Type;
                var rightType = rightValueBinder.Type;

                if (leftType == rightType)
                {
                    return new ComparisonCondition(leftOperand, rightOperand, ComparisonTypes.EqualTo);
                }
                else
                {
                    return new AlwaysFalseCondition();
                }
            }
            else if (firstValueBinder is CoalesceValueBinder coalesceValueBinder)
            {
                var disjunctionConditions = new List<IFilterCondition>();
                var binders = coalesceValueBinder.ValueBinders.ToArray();

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
            else if (firstValueBinder is SwitchValueBinder switchValueBinder)
            {
                return new DisjunctionCondition(switchValueBinder.Cases.Select(curCase => new ConjunctionCondition(new IFilterCondition[]
                {
                    new ComparisonCondition(new ColumnExpression(switchValueBinder.CaseVariable, false), new ConstantExpression(curCase.CaseValue, context), ComparisonTypes.EqualTo),
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
        public IFilterCondition CreateJoinEqualCondition(IValueBinder valueBinder, IValueBinder otherValueBinder, IQueryContext context)
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
        public ExpressionsSet CreateExpression(IQueryContext context, IValueBinder valueBinder)
        {
            return _valueBinderCreateExpression.CreateExpression(context, valueBinder);
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="node">The node.</param>
        public ExpressionsSet CreateExpression(IQueryContext context, INode node)
        {
            if (node is UriNode uriNode)
            {
                return new ConstantExpression(uriNode.Uri, context);
            }
            else if (node is LiteralNode literalNode)
            {
                return CreateLiteralExpression(context, literalNode);
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
        private IExpression CreateLiteralExpression(IQueryContext context, LiteralNode node)
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
                    case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                    case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                        return new ConstantExpression(double.Parse(node.Value, CultureInfo.InvariantCulture), context);
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
        public IFilterCondition CreateCondition(ISparqlCondition condition, IQueryContext context, IEnumerable<IValueBinder> valueBinders)
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
        public IExpression CreateExpression(IQueryContext context, ISparqlExpression expression, List<IValueBinder> valueBinders)
        {
            return _sparqlExpressionCreateExpression.CreateExpression(expression, context, valueBinders);
        }

        /// <summary>
        /// Creates the is bound condition.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        public IFilterCondition CreateIsBoundCondition(IExpression expression, IQueryContext context)
        {
            return _expressionIsBoundCondition.CreateIsBoundCondition(expression, context);
        }

        /// <summary>
        /// Creates the order by expression.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="data">The data.</param>
        /// <returns>IEnumerable&lt;IExpression&gt;.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<IExpression> CreateOrderByExpression(string variable, IEnumerable<IValueBinder> valueBinders, IQueryContext data)
        {
            // TODO: Handle order specifics
            // http://www.w3.org/TR/2013/REC-sparql11-query-20130321/#modOrderBy

            foreach (var valueBinder in valueBinders)
            {
                if (valueBinder.VariableName == variable)
                {
                    yield return CreateExpression(data, valueBinder);
                }
            }
        }
    }
}
