using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Query.Patterns;
using FilterPattern = Slp.Evi.Storage.Sparql.Algebra.Patterns.FilterPattern;
using GraphPattern = Slp.Evi.Storage.Sparql.Algebra.Patterns.GraphPattern;
using TriplePattern = Slp.Evi.Storage.Sparql.Algebra.Patterns.TriplePattern;

namespace Slp.Evi.Storage.Relational.Builder
{
    /// <summary>
    /// Relational builder
    /// </summary>
    public class RelationalBuilder
        : IModifierVisitor, IGraphPatternVisitor
    {
        /// <summary>
        /// The condition builder
        /// </summary>
        private readonly ConditionBuilder _conditionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalBuilder"/> class.
        /// </summary>
        public RelationalBuilder()
        {
            _conditionBuilder = new ConditionBuilder();
        }

        /// <summary>
        /// Processes the specified algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The context.</param>
        /// <returns>RelationalQuery.</returns>
        /// <exception cref="System.ArgumentException">Unexpected type;algebra</exception>
        public RelationalQuery Process(ISparqlQuery algebra, IQueryContext context)
        {
            if (algebra is IModifier modifier)
            {
                return (RelationalQuery)modifier.Accept(this, context);
            }
            else if (algebra is IGraphPattern graphPattern)
            {
                return (RelationalQuery)graphPattern.Accept(this, context);
            }
            else
            {
                throw new ArgumentException("Unexpected type", nameof(algebra));
            }
        }

        #region PatternVisitor
        /// <summary>
        /// Visits <see cref="EmptyPattern" />
        /// </summary>
        /// <param name="emptyPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyPattern emptyPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(new RelationalQuery(
                new CalculusModel(
                    new ICalculusVariable[] {},
                    new ICondition[] {}),
                new IValueBinder[] {}));
        }

        /// <summary>
        /// Visits <see cref="Sparql.Algebra.Patterns.FilterPattern" />
        /// </summary>
        /// <param name="filterPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(FilterPattern filterPattern, object data)
        {
            var inner = (RelationalQuery)filterPattern.InnerPattern.Accept(this, data);
            var innerModel = inner.Model as CalculusModel;

            if (innerModel == null)
            {
                throw new Exception($"Only {nameof(CalculusModel)} is expected");
            }

            var condition = _conditionBuilder.CreateCondition(filterPattern.Condition, (IQueryContext)data, inner.ValueBinders);

            var conditions = new List<ICondition>();
            conditions.AddRange(innerModel.SourceConditions);
            conditions.AddRange(innerModel.AssignmentConditions);
            conditions.AddRange(innerModel.FilterConditions);
            conditions.Add(condition);

            var model = new CalculusModel(innerModel.Variables, conditions);
            var query = new RelationalQuery(model, inner.ValueBinders);

            return ((IQueryContext) data).QueryPostProcesses.PostProcess(query);
        }

        /// <summary>
        /// Visits <see cref="NotMatchingPattern" />
        /// </summary>
        /// <param name="notMatchingPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NotMatchingPattern notMatchingPattern, object data)
        {
            return ((IQueryContext)data).QueryPostProcesses.PostProcess(new RelationalQuery(
                new CalculusModel(
                    new ICalculusVariable[] { },
                    new ICondition[] { new AlwaysFalseCondition() }),
                notMatchingPattern.Variables.Select(x => new EmptyValueBinder(x))));
        }

        /// <summary>
        /// Visits <see cref="Sparql.Algebra.Patterns.GraphPattern" />
        /// </summary>
        /// <param name="graphPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(GraphPattern graphPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="JoinPattern" />
        /// </summary>
        /// <param name="joinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(JoinPattern joinPattern, object data)
        {
            var relationalQueries = joinPattern.JoinedGraphPatterns.Select(x => x.Accept(this, data)).Cast<RelationalQuery>().ToList();

            Dictionary<string, IValueBinder> valueBinders = new Dictionary<string, IValueBinder>();
            List<ICondition> conditions = new List<ICondition>();

            foreach (var relationalQuery in relationalQueries)
            {
                var model = relationalQuery.Model as CalculusModel;

                if (model == null)
                {
                    throw new Exception($"Only {nameof(CalculusModel)} is expected");
                }

                conditions.AddRange(model.AssignmentConditions);
                conditions.AddRange(model.FilterConditions);
                conditions.AddRange(model.SourceConditions);

                foreach (var valueBinder in relationalQuery.ValueBinders)
                {
                    if (valueBinders.ContainsKey(valueBinder.VariableName))
                    {
                        var otherValueBinder = valueBinders[valueBinder.VariableName];
                        conditions.Add(_conditionBuilder.CreateJoinEqualCondition(valueBinder, otherValueBinder, (IQueryContext) data));

                        valueBinders[valueBinder.VariableName] = new CoalesceValueBinder(valueBinder.VariableName, otherValueBinder, valueBinder);
                    }
                    else
                    {
                        valueBinders.Add(valueBinder.VariableName, valueBinder);
                    }
                }
            }

            var finalValueBinders = valueBinders.Values.ToList();
            var neededVariables = finalValueBinders.SelectMany(x => x.NeededCalculusVariables).Distinct().ToArray();
            var calculusModel = new CalculusModel(neededVariables, conditions);

            return ((IQueryContext)data).QueryPostProcesses.PostProcess(new RelationalQuery(calculusModel, finalValueBinders));
        }

        /// <summary>
        /// Visits <see cref="LeftJoinPattern" />
        /// </summary>
        /// <param name="leftJoinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(LeftJoinPattern leftJoinPattern, object data)
        {
            var leftQuery = (RelationalQuery)leftJoinPattern.LeftOperand.Accept(this, data);
            var rightQuery = (RelationalQuery) leftJoinPattern.RightOperand.Accept(this, data);

            var leftQueryModel = leftQuery.Model as CalculusModel;

            if (leftQueryModel == null)
            {
                throw new Exception($"Only {nameof(CalculusModel)} is expected");
            }

            List<ICondition> conditions = new List<ICondition>();

            conditions.AddRange(leftQueryModel.AssignmentConditions);
            conditions.AddRange(leftQueryModel.FilterConditions);
            conditions.AddRange(leftQueryModel.SourceConditions);

            List<IFilterCondition> joinConditions = new List<IFilterCondition>();
            Dictionary<string, IValueBinder> valueBinders = new Dictionary<string, IValueBinder>();

            foreach (var valueBinder in leftQuery.ValueBinders)
            {
                valueBinders.Add(valueBinder.VariableName, valueBinder);
            }

            foreach (var valueBinder in rightQuery.ValueBinders)
            {
                if (valueBinders.ContainsKey(valueBinder.VariableName))
                {
                    var otherValueBinder = valueBinders[valueBinder.VariableName];
                    joinConditions.Add(_conditionBuilder.CreateJoinEqualCondition(valueBinder, otherValueBinder,
                        (IQueryContext) data));

                    valueBinders[valueBinder.VariableName] = new CoalesceValueBinder(valueBinder.VariableName, otherValueBinder, valueBinder);
                }
                else
                {
                    valueBinders.Add(valueBinder.VariableName, valueBinder);
                }
            }

            joinConditions.Add(_conditionBuilder.CreateCondition(leftJoinPattern.Condition, (IQueryContext)data, valueBinders.Values));

            var leftJoinCondition = new LeftJoinCondition(rightQuery.Model, joinConditions, rightQuery.Model.Variables);
            conditions.Add(leftJoinCondition);

            List<ICalculusVariable> variables = new List<ICalculusVariable>();
            variables.AddRange(leftQuery.Model.Variables);
            variables.AddRange(rightQuery.Model.Variables);

            var model = new CalculusModel(variables, conditions);
            return ((IQueryContext)data).QueryPostProcesses.PostProcess(new RelationalQuery(model, valueBinders.Values.ToList()));
        }

        /// <summary>
        /// Visits <see cref="MinusPattern" />
        /// </summary>
        /// <param name="minusPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(MinusPattern minusPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="Sparql.Algebra.Patterns.TriplePattern" />
        /// </summary>
        /// <param name="triplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        /// <exception cref="System.InvalidOperationException">The triple pattern should not be present when transforming to relational form.</exception>
        public object Visit(TriplePattern triplePattern, object data)
        {
            throw new InvalidOperationException("The triple pattern should not be present when transforming to relational form.");
        }

        /// <summary>
        /// Visits <see cref="UnionPattern" />
        /// </summary>
        /// <param name="unionPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionPattern unionPattern, object data)
        {
            var context = (IQueryContext) data;
            var relationalQueries = unionPattern.UnionedGraphPatterns.Select(x => x.Accept(this, data)).Cast<RelationalQuery>().ToList();

            List<ICalculusSource> sources = new List<ICalculusSource>();
            List<ICalculusVariable> variables = new List<ICalculusVariable>();
            Dictionary<string, SwitchValueBinder> valueBinders = new Dictionary<string, SwitchValueBinder>();

            var caseVariable = new AssignedVariable(context.Db.SqlTypeForInt);
            variables.Add(caseVariable);

            int counter = 0;

            foreach (var relationalQuery in relationalQueries)
            {
                var oldModel = relationalQuery.Model as CalculusModel;

                if (oldModel == null)
                {
                    throw new Exception($"Only {nameof(CalculusModel)} is expected");
                }

                variables.AddRange(oldModel.Variables);

                var newConditions = new List<ICondition>();
                newConditions.AddRange(oldModel.AssignmentConditions);
                newConditions.AddRange(oldModel.FilterConditions);
                newConditions.AddRange(oldModel.SourceConditions);

                var caseValue = counter++;
                var caseValueExpression = new ConstantExpression(caseValue, context);
                var caseAssignmentCondition = new AssignmentFromExpressionCondition(caseVariable, caseValueExpression);

                newConditions.Add(caseAssignmentCondition);
                
                var newVariables = new List<ICalculusVariable>(oldModel.Variables);
                newVariables.Add(caseVariable);

                var newModel = new CalculusModel(newVariables, newConditions);
                sources.Add(newModel);

                foreach (var valueBinder in relationalQuery.ValueBinders)
                {
                    var variableName = valueBinder.VariableName;

                    var thisCase = new SwitchValueBinder.Case(caseValue, valueBinder);

                    if (valueBinders.ContainsKey(variableName))
                    {
                        var oldValueBinder = valueBinders[variableName];
                        var cases = new List<SwitchValueBinder.Case>(oldValueBinder.Cases);
                        cases.Add(thisCase);

                        valueBinders[variableName] = new SwitchValueBinder(variableName, caseVariable, cases);
                    }
                    else
                    {
                        valueBinders.Add(variableName, new SwitchValueBinder(variableName, caseVariable, new SwitchValueBinder.Case[] { thisCase }));
                    }
                }
            }

            var unionedSourcesCondition = new UnionedSourcesCondition(caseVariable, variables, sources);
            var model = new CalculusModel(variables, new ICondition[] { unionedSourcesCondition });
            return new RelationalQuery(model, valueBinders.Values.ToList());
        }

        /// <summary>
        /// Visits <see cref="RestrictedTriplePattern" />
        /// </summary>
        /// <param name="restrictedTriplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(RestrictedTriplePattern restrictedTriplePattern, object data)
        {
            var context = (IQueryContext) data;
            List<ICondition> conditions = new List<ICondition>();
            List<IValueBinder> valueBinders = new List<IValueBinder>();

            ISqlCalculusSource source;
            ISqlCalculusSource refSource;
            
            ProcessTriplePatternSource(restrictedTriplePattern, conditions, out source, out refSource, context);
            ProcessTriplePatternSubject(restrictedTriplePattern, conditions, valueBinders, source, context);
            ProcessTriplePatternPredicate(restrictedTriplePattern, conditions, valueBinders, source, context);

            if (restrictedTriplePattern.RefObjectMap != null)
            {
                ProcessTriplePatternRefObject(restrictedTriplePattern, conditions, valueBinders, refSource, context);

                conditions.Add(new TupleFromSourceCondition(refSource.Variables, refSource));
            }
            else
            {
                ProcessTriplePatternObject(restrictedTriplePattern, conditions, valueBinders, source, context);
            }

            conditions.Add(new TupleFromSourceCondition(source.Variables, source));

            return ((IQueryContext)data).QueryPostProcesses.PostProcess(new RelationalQuery(
                new CalculusModel(
                    valueBinders.SelectMany(x => x.NeededCalculusVariables).Distinct(),
                    conditions),
                valueBinders));
        }

        /// <summary>
        /// Visits <see cref="ExtendPattern"/>
        /// </summary>
        /// <param name="extendPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExtendPattern extendPattern, object data)
        {
            var context = (IQueryContext)data;
            var inner = Process(extendPattern.InnerPattern, context);
            var model = inner.Model;
            var valueBinders = inner.ValueBinders.ToList();

            var expression = _conditionBuilder.CreateExpression(context, extendPattern.Expression, valueBinders);

            if (valueBinders.Any(x => x.VariableName == extendPattern.VariableName))
            {
                throw new InvalidOperationException("Tried to extend an already existing variable");
            }
            else
            {
                var newValueBinder = new ExpressionSetValueBinder(extendPattern.VariableName, expression);
                valueBinders.Add(newValueBinder);

                return ((IQueryContext) data).QueryPostProcesses.PostProcess(new RelationalQuery(model, valueBinders));
            }
        }

        #endregion

        #region ModifierVisitor
        /// <summary>
        /// Visits <see cref="SelectModifier" />
        /// </summary>
        /// <param name="selectModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SelectModifier selectModifier, object data)
        {
            var inner = Process(selectModifier.InnerQuery, (IQueryContext) data);

            Dictionary<string, IValueBinder> providedValueBinders = inner.ValueBinders.ToDictionary(valueBinder => valueBinder.VariableName);

            var valueBinders = new List<IValueBinder>();

            foreach (var variable in selectModifier.Variables)
            {
                valueBinders.Add(providedValueBinders.ContainsKey(variable)
                    ? providedValueBinders[variable]
                    : new EmptyValueBinder(variable));
            }

            return ((IQueryContext)data).QueryPostProcesses.PostProcess(new RelationalQuery(inner.Model, valueBinders));
        }

        /// <summary>
        /// Visits <see cref="OrderByModifier"/>
        /// </summary>
        /// <param name="orderByModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(OrderByModifier orderByModifier, object data)
        {
            var inner = Process(orderByModifier.InnerQuery, (IQueryContext)data);
            var orderings = ProcessOrdering(inner.ValueBinders, orderByModifier.Ordering, (IQueryContext) data);

            if (inner.Model is ModifiedCalculusModel modifiedCalculusModel)
            {
                orderings.AddRange(modifiedCalculusModel.Ordering);
                var newModel = new ModifiedCalculusModel(modifiedCalculusModel.InnerModel, orderings, modifiedCalculusModel.Limit, modifiedCalculusModel.Offset);

                return new RelationalQuery(newModel, inner.ValueBinders);
            }
            else if (inner.Model is CalculusModel calculusModel)
            {
                var newModel = new ModifiedCalculusModel(calculusModel, orderings,
                    null, null);

                return new RelationalQuery(newModel, inner.ValueBinders);
            }
            else
            {
                throw new Exception($"Expected {nameof(ModifiedCalculusModel)} or {nameof(CalculusModel)}");
            }
        }

        /// <summary>
        /// Processes the ordering.
        /// </summary>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="ordering">The ordering.</param>
        /// <param name="data">The data.</param>
        /// <returns>List&lt;ModifiedCalculusModel.OrderingPart&gt;.</returns>
        private List<ModifiedCalculusModel.OrderingPart> ProcessOrdering(IEnumerable<IValueBinder> valueBinders, IEnumerable<OrderByModifier.OrderingPart> ordering, IQueryContext data)
        {
            List<ModifiedCalculusModel.OrderingPart> result = new List<ModifiedCalculusModel.OrderingPart>();

            foreach (var orderingPart in ordering)
            {
                var transformed = _conditionBuilder.CreateOrderByExpression(orderingPart.Variable, valueBinders, data);

                foreach (var expression in transformed)
                {
                    result.Add(new ModifiedCalculusModel.OrderingPart(expression, orderingPart.IsDescending));
                }
            }

            return result;
        }

        /// <summary>
        /// Visits <see cref="SliceModifier"/>
        /// </summary>
        /// <param name="sliceModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SliceModifier sliceModifier, object data)
        {
            var inner = Process(sliceModifier.InnerQuery, (IQueryContext) data);

            if (inner.Model is ModifiedCalculusModel modifiedCalculusModel)
            {
                int? limit = modifiedCalculusModel.Limit;
                int? offset = modifiedCalculusModel.Offset;

                if (sliceModifier.Limit.HasValue)
                {
                    if (limit.HasValue)
                    {
                        limit = Math.Min(limit.Value, sliceModifier.Limit.Value);
                    }
                    else
                    {
                        limit = sliceModifier.Limit.Value;
                    }
                }

                if (sliceModifier.Offset.HasValue)
                {
                    if (offset.HasValue)
                    {
                        offset = offset.Value + sliceModifier.Offset.Value;
                    }
                    else
                    {
                        offset = sliceModifier.Offset.Value;
                    }
                }

                var newModel = new ModifiedCalculusModel(modifiedCalculusModel.InnerModel, modifiedCalculusModel.Ordering, limit, offset);

                return new RelationalQuery(newModel, inner.ValueBinders);
            }
            else if (inner.Model is CalculusModel calculusModel)
            {
                var newModel = new ModifiedCalculusModel(calculusModel, new List<ModifiedCalculusModel.OrderingPart>(),
                    sliceModifier.Limit, sliceModifier.Offset);

                return new RelationalQuery(newModel, inner.ValueBinders);
            }
            else
            {
                throw new Exception($"Expected {nameof(ModifiedCalculusModel)} or {nameof(CalculusModel)}");
            }
        }

        #endregion

        /// <summary>
        /// Processes the triple pattern source.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="source">The source.</param>
        /// <param name="refSource">The reference source.</param>
        /// <param name="context">The context.</param>
        private void ProcessTriplePatternSource(RestrictedTriplePattern triplePattern, List<ICondition> conditions, out ISqlCalculusSource source, out ISqlCalculusSource refSource, IQueryContext context)
        {
            refSource = null;

            source = GetTripleMapSource(triplePattern.TripleMap, context);

            if (triplePattern.RefObjectMap != null)
            {
                refSource = GetTripleMapSource(triplePattern.RefObjectMap.ParentTriplesMap, context);

                foreach (var joinCondition in triplePattern.RefObjectMap.JoinConditions)
                {
                    var sourceCalculusVariable = source.GetVariable(joinCondition.ChildColumn);
                    var refSourceCalculusVariable = refSource.GetVariable(joinCondition.ParentColumn);

                    conditions.Add(new EqualVariablesCondition(sourceCalculusVariable, refSourceCalculusVariable));
                }
            }
        }

        /// <summary>
        /// Gets the triple map source.
        /// </summary>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="context">The context.</param>
        /// <returns>ISqlCalculusSource.</returns>
        /// <exception cref="System.ArgumentException">Unknown source;tripleMap</exception>
        private static ISqlCalculusSource GetTripleMapSource(ITriplesMap tripleMap, IQueryContext context)
        {
            var sqlTableName = context.Mapping.Cache.GetSqlTable(tripleMap);
            var sqlStatement = context.Mapping.Cache.GetSqlStatement(tripleMap);

            if (!string.IsNullOrEmpty(sqlStatement))
            {
                throw new NotImplementedException();
            }
            else if (!string.IsNullOrEmpty(sqlTableName))
            {
                return new SqlTable(context.SchemaProvider.GetTableInfo(sqlTableName));
            }
            else
            {
                throw new ArgumentException("Unknown source", nameof(tripleMap));
            }
        }

        /// <summary>
        /// Processes the triple pattern predicate.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternPredicate(RestrictedTriplePattern triplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, IQueryContext context)
        {
            ProcessTriplePatternItem(triplePattern.PredicatePattern, triplePattern.PredicateMap, conditions, valueBinders, source, context);
        }

        /// <summary>
        /// Processes the triple pattern item.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="termMap">The term map</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The context.</param>
        private void ProcessTriplePatternItem(PatternItem pattern, ITermMap termMap, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, IQueryContext context)
        {
            if (pattern is VariablePattern)
            {
                ProcessTriplePatternVariable(pattern.VariableName, termMap, conditions, valueBinders, source, context);
            }
            else if (pattern is NodeMatchPattern nodeMatchPattern)
            {
                var node = nodeMatchPattern.Node;

                ProcessTriplePatternCondition(node, termMap, conditions, source, context);
            }
            else if (pattern is BlankNodePattern blankNodePattern)
            {
                ProcessTriplePatternVariable(blankNodePattern.ID, termMap, conditions, valueBinders,
                    source, context);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Processes the triple pattern condition.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="termMap">The term map.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The context.</param>
        private void ProcessTriplePatternCondition(INode node, ITermMap termMap, List<ICondition> conditions, ISqlCalculusSource source, IQueryContext context)
        {
            var valueBinder = new BaseValueBinder(null, termMap, source, context.TypeCache);
            var notNullCondition = _conditionBuilder.CreateIsBoundCondition(valueBinder, context);
            conditions.Add(notNullCondition);

            var condition = _conditionBuilder.CreateEqualsCondition(node, valueBinder, context);
            conditions.Add(condition);
        }

        /// <summary>
        /// Processes the triple pattern variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="termMap">The term map.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The context.</param>
        private void ProcessTriplePatternVariable(string variableName, ITermMap termMap, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, IQueryContext context)
        {
            var valueBinder = new BaseValueBinder(variableName, termMap, source, context.TypeCache);

            var notNullCondition = _conditionBuilder.CreateIsBoundCondition(valueBinder, context);
            conditions.Add(notNullCondition);

            var sameVariableValueBinder = valueBinders.FirstOrDefault(x => x.VariableName == variableName);

            if (sameVariableValueBinder == null)
            {
                valueBinders.Add(valueBinder);
            }
            else
            {
                var condition = _conditionBuilder.CreateEqualsCondition(valueBinder, sameVariableValueBinder, context);
                conditions.Add(condition);
            }
        }

        /// <summary>
        /// Processes the triple pattern subject.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternSubject(RestrictedTriplePattern triplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, IQueryContext context)
        {
            ProcessTriplePatternItem(triplePattern.SubjectPattern, triplePattern.SubjectMap, conditions, valueBinders, source, context);
        }

        /// <summary>
        /// Processes the triple pattern object.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternObject(RestrictedTriplePattern triplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, IQueryContext context)
        {
            ProcessTriplePatternItem(triplePattern.ObjectPattern, triplePattern.ObjectMap, conditions, valueBinders, source, context);
        }

        /// <summary>
        /// Processes the triple pattern reference object.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="refSource">The reference source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternRefObject(RestrictedTriplePattern triplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource refSource, IQueryContext context)
        {
            ProcessTriplePatternItem(triplePattern.ObjectPattern, triplePattern.RefObjectMap.SubjectMap, conditions, valueBinders, refSource, context);
        }
    }
}
