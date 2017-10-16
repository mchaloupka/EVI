using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
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
        /// <param name="valueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(BaseValueBinder valueBinder, object data)
        {
            var context = (IQueryContext) data;
            var map = valueBinder.TermMap;

            if (map.IsConstantValued)
            {
                if (map is IUriValuedTermMap uriValuedTermMap)
                {
                    return new ConstantExpression(uriValuedTermMap.URI, context);
                }
                else if (map is IObjectMap objectMap)
                {
                    if (objectMap.URI != null)
                    {
                        return new ConstantExpression(objectMap.URI, context);
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
                return new ColumnExpression(valueBinder.GetCalculusVariable(map.ColumnName), map.TermType.IsURI);
            }
            else if (map.IsTemplateValued)
            {
                List<IExpression> parts = new List<IExpression>();

                foreach (var templatePart in valueBinder.TemplateParts)
                {
                    if (templatePart.IsColumn)
                    {
                        parts.Add(new ColumnExpression(valueBinder.GetCalculusVariable(templatePart.Column),
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
            List<IExpression> expressions = new List<IExpression>();

            foreach (var binder in coalesceValueBinder.ValueBinders)
            {
                expressions.Add(CreateExpression(context, binder));
            }

            return new CoalesceExpression(expressions);
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
            var statements = new List<CaseExpression.Statement>();

            foreach (var @case in switchValueBinder.Cases)
            {
                var expression = CreateExpression(context, @case.ValueBinder);
                var condition = new ComparisonCondition(new ColumnExpression(switchValueBinder.CaseVariable, false), new ConstantExpression(@case.CaseValue, context), ComparisonTypes.EqualTo);
                statements.Add(new CaseExpression.Statement(condition, expression));
            }

            return new CaseExpression(statements);
        }

        /// <summary>
        /// Visits <see cref="ExpressionValueBinder"/>
        /// </summary>
        /// <param name="expressionValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExpressionValueBinder expressionValueBinder, object data)
        {
            throw new NotImplementedException();
        }
    }
}