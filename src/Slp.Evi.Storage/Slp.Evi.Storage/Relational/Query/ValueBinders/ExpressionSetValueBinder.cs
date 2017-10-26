using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers;
using Slp.Evi.Storage.Relational.Query.Utils;
using Slp.Evi.Storage.Sparql.Types;
using VDS.RDF;

namespace Slp.Evi.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// Value binder based on SQL expression.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.IValueBinder" />
    public class ExpressionSetValueBinder
        : IValueBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionSetValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="expressionSet">The expression set.</param>
        public ExpressionSetValueBinder(string variableName, ExpressionsSet expressionSet)
        {
            VariableName = variableName;
            ExpressionSet = expressionSet;
            NeededCalculusVariables =
                expressionSet.TypeCategoryExpression.UsedCalculusVariables
                    .Union(expressionSet.TypeExpression.UsedCalculusVariables)
                    .Union(expressionSet.StringExpression.UsedCalculusVariables)
                    .Union(expressionSet.NumericExpression.UsedCalculusVariables)
                    .Union(expressionSet.BooleanExpression.UsedCalculusVariables)
                    .Union(expressionSet.DateTimeExpression.UsedCalculusVariables)
                    .Distinct()
                    .ToList();
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
        /// Loads the node.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        public INode LoadNode(INodeFactory nodeFactory, IQueryResultRow rowData, IQueryContext context)
        {
            // TODO: Correct type
            var staticEvaluator = new StaticEvaluator();
            var typeIndex = staticEvaluator.Evaluate(ExpressionSet.TypeExpression, rowData, context) as int?;

            if (!typeIndex.HasValue)
            {
                return null;
            }
            else
            {
                var type = context.TypeCache.GetValueType(typeIndex.Value);
                switch (type.Category)
                {
                    case TypeCategories.BlankNode:
                        var blankNodeId = staticEvaluator.Evaluate(ExpressionSet.StringExpression, rowData, context);
                        return context.GetBlankNodeForValue(nodeFactory, blankNodeId);
                    case TypeCategories.IRI:
                        var iri = staticEvaluator.Evaluate(ExpressionSet.StringExpression, rowData, context).ToString();
                        return nodeFactory.CreateUriNode(new Uri(iri));
                    case TypeCategories.SimpleLiteral:
                        var litValue = staticEvaluator.Evaluate(ExpressionSet.StringExpression, rowData, context).ToString();
                        return nodeFactory.CreateLiteralNode(litValue);
                    case TypeCategories.NumericLiteral:
                        var numValue = staticEvaluator.Evaluate(ExpressionSet.NumericExpression, rowData, context).ToString();
                        return nodeFactory.CreateLiteralNode(numValue, ((LiteralValueType) type).LiteralType);
                    case TypeCategories.StringLiteral:
                        var stringValue = staticEvaluator.Evaluate(ExpressionSet.StringExpression, rowData, context).ToString();
                        return nodeFactory.CreateLiteralNode(stringValue, ((LiteralValueType)type).LiteralType);
                    case TypeCategories.BooleanLiteral:
                        var booleanValue = staticEvaluator.Evaluate(ExpressionSet.BooleanExpression, rowData, context).ToString();
                        return nodeFactory.CreateLiteralNode(booleanValue, ((LiteralValueType)type).LiteralType);
                    case TypeCategories.DateTimeLiteral:
                        var dateTimeValue = staticEvaluator.Evaluate(ExpressionSet.DateTimeExpression, rowData, context).ToString();
                        return nodeFactory.CreateLiteralNode(dateTimeValue, ((LiteralValueType)type).LiteralType);
                    case TypeCategories.OtherLiterals:
                        var otherValue = staticEvaluator.Evaluate(ExpressionSet.StringExpression, rowData, context).ToString();
                        var literalValueType = (LiteralValueType) type;
                        if (literalValueType.LanguageTag == null)
                        {
                            return nodeFactory.CreateLiteralNode(otherValue, ((LiteralValueType)type).LiteralType);
                        }
                        else
                        {
                            return nodeFactory.CreateLiteralNode(otherValue, literalValueType.LanguageTag);
                        }
                    default:
                        throw new ArgumentException($"The type category {type.Category} is not supported");
                }
            }
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public ExpressionsSet ExpressionSet { get; }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        public IEnumerable<ICalculusVariable> NeededCalculusVariables { get; }
    }
}
