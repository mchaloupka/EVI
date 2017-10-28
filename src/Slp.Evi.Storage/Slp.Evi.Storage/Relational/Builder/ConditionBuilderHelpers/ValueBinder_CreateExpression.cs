using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Types;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// Helper for <see cref="ConditionBuilder.CreateExpression(Slp.Evi.Storage.Query.IQueryContext,Slp.Evi.Storage.Relational.Query.IValueBinder)"/>.
    /// </summary>
    public class ValueBinder_CreateExpression
        : IValueBinderVisitor
    {
        /// <summary>
        /// The condition builder
        /// </summary>
        private readonly ConditionBuilder _conditionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBinder_CreateExpression"/> class.
        /// </summary>
        /// <param name="conditionBuilder">The condition builder.</param>
        public ValueBinder_CreateExpression(ConditionBuilder conditionBuilder)
        {
            _conditionBuilder = conditionBuilder;
        }

        /// <summary>
        /// Creates the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="valueBinder">The value binder.</param>
        public ExpressionsSet CreateExpression(IQueryContext context, IValueBinder valueBinder)
        {
            return (ExpressionsSet) valueBinder.Accept(this, context);
        }

        /// <summary>
        /// Visits <see cref="BaseValueBinder"/>
        /// </summary>
        /// <param name="baseValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            var context = (IQueryContext) data;
            var map = baseValueBinder.TermMap;
            var type = context.TypeCache.GetValueType(map);

            if (map.IsConstantValued)
            {
                if (map is IUriValuedTermMap uriValuedTermMap)
                {
                    return new ExpressionsSet(
                        new ConstantExpression(context.TypeCache.GetIndex(type), context),
                        new ConstantExpression((int)type.Category, context),
                        new ConstantExpression(uriValuedTermMap.URI, context),
                        null,
                        null,
                        null,
                        context);
                }
                else if (map is IObjectMap objectMap)
                {
                    if (objectMap.URI != null)
                    {
                        return new ExpressionsSet(
                            new ConstantExpression(context.TypeCache.GetIndex(type), context),
                            new ConstantExpression((int)type.Category, context),
                            new ConstantExpression(objectMap.URI, context),
                            null,
                            null,
                            null,
                            context);
                    }
                    else if (objectMap.Literal != null)
                    {
                        var parsed = objectMap.Parsed();

                        if (parsed.Type != null)
                        {
                            var node = context.NodeFactory.CreateLiteralNode(parsed.Value, parsed.Type);
                            return _conditionBuilder.CreateExpression(context, node);
                        }
                        else if (parsed.LanguageTag != null)
                        {
                            var node = context.NodeFactory.CreateLiteralNode(parsed.Value, parsed.LanguageTag);
                            return _conditionBuilder.CreateExpression(context, node);
                        }
                        else
                        {
                            var node = context.NodeFactory.CreateLiteralNode(parsed.Value);
                            return _conditionBuilder.CreateExpression(context, node);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Object map's value must be IRI or literal.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unknown constant valued term map");
                }
            }
            else if (map.IsColumnValued)
            {
                switch (type.Category)
                {
                    case TypeCategories.NumericLiteral:
                        return new ExpressionsSet(
                            new ConstantExpression(context.TypeCache.GetIndex(type), context),
                            new ConstantExpression((int)type.Category, context),
                            null,
                            new ColumnExpression(baseValueBinder.GetCalculusVariable(map.ColumnName), map.TermType.IsURI),
                            null,
                            null,
                            context);
                    case TypeCategories.BooleanLiteral:
                        return new ExpressionsSet(
                            new ConstantExpression(context.TypeCache.GetIndex(type), context),
                            new ConstantExpression((int)type.Category, context),
                            null,
                            null,
                            new ColumnExpression(baseValueBinder.GetCalculusVariable(map.ColumnName), map.TermType.IsURI),
                            null,
                            context);
                    case TypeCategories.DateTimeLiteral:
                        return new ExpressionsSet(
                            new ConstantExpression(context.TypeCache.GetIndex(type), context),
                            new ConstantExpression((int)type.Category, context),
                            null,
                            null,
                            null,
                            new ColumnExpression(baseValueBinder.GetCalculusVariable(map.ColumnName), map.TermType.IsURI),
                            context);
                    default:
                        return new ExpressionsSet(
                            new ConstantExpression(context.TypeCache.GetIndex(type), context),
                            new ConstantExpression((int)type.Category, context),
                            new ColumnExpression(baseValueBinder.GetCalculusVariable(map.ColumnName), map.TermType.IsURI),
                            null,
                            null,
                            null,
                            context);
                }
            }
            else if (map.IsTemplateValued)
            {
                List<IExpression> parts = new List<IExpression>();

                foreach (var templatePart in baseValueBinder.TemplateParts)
                {
                    if (templatePart.IsColumn)
                    {
                        parts.Add(new ColumnExpression(baseValueBinder.GetCalculusVariable(templatePart.Column),
                            map.TermType.IsURI));
                    }
                    else if (templatePart.IsText)
                    {
                        parts.Add(new ConstantExpression(templatePart.Text, context));
                    }
                    else
                    {
                        throw new InvalidOperationException("Must be column or constant");
                    }
                }

                if (parts.Count == 0)
                {
                    return new ExpressionsSet(
                        new ConstantExpression(context.TypeCache.GetIndex(type), context),
                        new ConstantExpression((int)type.Category, context),
                        new ConstantExpression(string.Empty, context),
                        null,
                        null,
                        null,
                        context);
                }
                else if (parts.Count == 1)
                {
                    return new ExpressionsSet(
                        new ConstantExpression(context.TypeCache.GetIndex(type), context),
                        new ConstantExpression((int)type.Category, context),
                        parts[0],
                        null,
                        null,
                        null,
                        context);
                }
                else
                {
                    return new ExpressionsSet(
                        new ConstantExpression(context.TypeCache.GetIndex(type), context),
                        new ConstantExpression((int)type.Category, context),
                        new ConcatenationExpression(parts, context.Db.SqlTypeForString),
                        null,
                        null,
                        null,
                        context);
                }
            }
            else
            {
                throw new InvalidOperationException("Mapping can be only constant, column or template valued");
            }
        }

        /// <summary>
        /// Visits <see cref="EmptyValueBinder"/>
        /// </summary>
        /// <param name="emptyValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyValueBinder emptyValueBinder, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="CoalesceValueBinder"/>
        /// </summary>
        /// <param name="coalesceValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            var context = (IQueryContext) data;

            var expressionsSets = coalesceValueBinder.ValueBinders
                .Select(x => CreateExpression(context, x)).ToList();

            return new ExpressionsSet(
                new CoalesceExpression(expressionsSets.Select(x => x.TypeExpression)),
                new CoalesceExpression(expressionsSets.Select(x => x.TypeCategoryExpression)),
                new CoalesceExpression(expressionsSets.Select(x => x.StringExpression)),
                new CoalesceExpression(expressionsSets.Select(x => x.NumericExpression)),
                new CoalesceExpression(expressionsSets.Select(x => x.BooleanExpression)),
                new CoalesceExpression(expressionsSets.Select(x => x.DateTimeExpression)),
                context);
        }

        /// <summary>
        /// Visits <see cref="SwitchValueBinder"/>
        /// </summary>
        /// <param name="switchValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            var context = (IQueryContext) data;
            var typeStatements = new List<CaseExpression.Statement>();
            var typeCategoryStatements = new List<CaseExpression.Statement>();
            var stringStatements = new List<CaseExpression.Statement>();
            var numericStatements = new List<CaseExpression.Statement>();
            var booleanStatements = new List<CaseExpression.Statement>();
            var datetimeStatements = new List<CaseExpression.Statement>();

            foreach (var @case in switchValueBinder.Cases)
            {
                var expression = CreateExpression(context, @case.ValueBinder);
                var condition = new ComparisonCondition(new ColumnExpression(switchValueBinder.CaseVariable, false), new ConstantExpression(@case.CaseValue, context), ComparisonTypes.EqualTo);

                typeStatements.Add(new CaseExpression.Statement(condition, expression.TypeExpression));
                typeCategoryStatements.Add(new CaseExpression.Statement(condition, expression.TypeCategoryExpression));
                stringStatements.Add(new CaseExpression.Statement(condition, expression.StringExpression));
                numericStatements.Add(new CaseExpression.Statement(condition, expression.NumericExpression));
                booleanStatements.Add(new CaseExpression.Statement(condition, expression.BooleanExpression));
                datetimeStatements.Add(new CaseExpression.Statement(condition, expression.DateTimeExpression));
            }

            return new ExpressionsSet(
                new CaseExpression(typeStatements),
                new CaseExpression(typeCategoryStatements),
                new CaseExpression(stringStatements),
                new CaseExpression(numericStatements),
                new CaseExpression(booleanStatements),
                new CaseExpression(datetimeStatements),
                context);
        }

        /// <summary>
        /// Visits <see cref="ExpressionSetValueBinder"/>
        /// </summary>
        /// <param name="expressionSetValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExpressionSetValueBinder expressionSetValueBinder, object data)
        {
            throw new NotImplementedException();
        }
    }
}