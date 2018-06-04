using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Common.Optimization.PatternMatching;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Types;

namespace Slp.Evi.Storage.Relational.Builder.ValueBinderHelpers
{
    /// <summary>
    /// The aligner for <see cref="IValueBinder"/>.
    /// </summary>
    public class ValueBinderAligner
    {
        private readonly ConditionBuilder _conditionBuilder;
        private readonly ValueBinderFlattener _valueBinderFlattener;
        private readonly PatternComparer _patternComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBinderAligner"/> class.
        /// </summary>
        /// <param name="conditionBuilder">The condition builder.</param>
        public ValueBinderAligner(ConditionBuilder conditionBuilder)
        {
            _conditionBuilder = conditionBuilder;
            _valueBinderFlattener = new ValueBinderFlattener(_conditionBuilder);
            _patternComparer = new PatternComparer();
        }

        /// <summary>
        /// Aligns the specified query.
        /// </summary>
        /// <param name="toAlign">Query to align.</param>
        /// <param name="queryContext">The query context.</param>
        public RelationalQuery Align(RelationalQuery toAlign, IQueryContext queryContext)
        {
            var valueBinders = toAlign.ValueBinders.ToList();
            var model = toAlign.Model;

            if (model is ModifiedCalculusModel modifiedCalculusModel)
            {
                var calculusModel = modifiedCalculusModel.InnerModel;
                var newModel = Align(calculusModel, valueBinders, queryContext);

                if (newModel.changed)
                {
                    var modifiedModel = new ModifiedCalculusModel(newModel.model, modifiedCalculusModel.Ordering,
                        modifiedCalculusModel.Limit, modifiedCalculusModel.Offset, modifiedCalculusModel.IsDistinct);

                    return queryContext.QueryPostProcesses.PostProcess(new RelationalQuery(modifiedModel, newModel.valueBinders));
                }
                else
                {
                    return toAlign;
                }

            }
            else if (model is CalculusModel calculusModel)
            {
                 var newModel = Align(calculusModel, valueBinders, queryContext);

                if (newModel.changed)
                {
                    return queryContext.QueryPostProcesses.PostProcess(new RelationalQuery(newModel.model, newModel.valueBinders));
                }
                else
                {
                    return toAlign;
                }
            }
            else
            {
                throw new Exception($"Expected {nameof(ModifiedCalculusModel)} or {nameof(CalculusModel)}");
            }
        }

        private (CalculusModel model, List<IValueBinder> valueBinders, bool changed) Align(CalculusModel calculusModel, List<IValueBinder> valueBinders, IQueryContext queryContext)
        {
            List<IValueBinder> resultingBinders = new List<IValueBinder>();
            List<IAssignmentCondition> assignmentConditions = new List<IAssignmentCondition>();
            bool changed = false;

            foreach (var valueBinder in valueBinders)
            {
                var flattened = _valueBinderFlattener.Flatten(valueBinder, queryContext).ToList();

                if (flattened.Select(x => x.valueBinder).Any(x => !(x is BaseValueBinder)))
                {
                    resultingBinders.Add(ConvertToExpressionSetValueBinder(valueBinder, queryContext));
                    changed = true;
                }
                else
                {
                    var groupedByType = flattened
                        .Select(x => (x.condition, (BaseValueBinder) x.valueBinder))
                        .GroupBy(x => x.Item2.Type)
                        .ToList();

                    var cases = new List<SwitchValueBinder.Case>();
                    var caseIndex = 0;
                    var caseStatements = new List<CaseExpression.Statement>();
                    var aligned = false;

                    foreach (var typeGroup in groupedByType)
                    {
                        if (typeGroup.Count() == 1)
                        {
                            var item = typeGroup.First();
                            caseStatements.Add(new CaseExpression.Statement(item.Item1, new ConstantExpression(caseIndex, queryContext)));
                            cases.Add(new SwitchValueBinder.Case(caseIndex++, item.Item2));
                        }
                        else
                        {
                            switch (typeGroup.Key.Category)
                            {
                                case TypeCategories.IRI:
                                    aligned = AlignIriValues(typeGroup.ToList(), assignmentConditions,
                                                  ref caseIndex, cases, caseStatements, queryContext) || aligned;
                                    break;
                                case TypeCategories.NumericLiteral:
                                    aligned = true;
                                    caseIndex = AlignLiteralValues(valueBinder, typeGroup.Key, typeGroup.ToList(),
                                        assignmentConditions, caseIndex, cases, caseStatements, queryContext,
                                        queryContext.Db.SqlTypeForDouble, x => x.NumericExpression);
                                    break;
                                case TypeCategories.BlankNode:
                                case TypeCategories.SimpleLiteral:
                                case TypeCategories.StringLiteral:
                                case TypeCategories.OtherLiterals:
                                    aligned = true;
                                    caseIndex = AlignLiteralValues(valueBinder, typeGroup.Key, typeGroup.ToList(),
                                        assignmentConditions, caseIndex, cases, caseStatements, queryContext,
                                        queryContext.Db.SqlTypeForString, x => x.StringExpression);
                                    break;
                                case TypeCategories.BooleanLiteral:
                                    aligned = true;
                                    caseIndex = AlignLiteralValues(valueBinder, typeGroup.Key, typeGroup.ToList(),
                                        assignmentConditions, caseIndex, cases, caseStatements, queryContext,
                                        queryContext.Db.SqlTypeForBoolean, x => x.BooleanExpression);
                                    break;
                                case TypeCategories.DateTimeLiteral:
                                    aligned = true;
                                    caseIndex = AlignLiteralValues(valueBinder, typeGroup.Key, typeGroup.ToList(),
                                        assignmentConditions, caseIndex, cases, caseStatements, queryContext,
                                        queryContext.Db.SqlTypeForDateTime, x => x.DateTimeExpression);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }

                    if (!aligned)
                    {
                        resultingBinders.Add(valueBinder);
                    }
                    else
                    {
                        var caseVariable = new AssignedVariable(queryContext.Db.SqlTypeForInt);
                        assignmentConditions.Add(new AssignmentFromExpressionCondition(caseVariable, new CaseExpression(caseStatements)));

                        resultingBinders.Add(new SwitchValueBinder(valueBinder.VariableName, caseVariable, cases));
                    }
                }
            }

            if (assignmentConditions.Count > 0)
            {
                var conditions = new List<ICondition>();
                conditions.AddRange(assignmentConditions);
                conditions.AddRange(calculusModel.AssignmentConditions);
                conditions.AddRange(calculusModel.FilterConditions);
                conditions.AddRange(calculusModel.SourceConditions);

                var variables = assignmentConditions.Select(x => x.Variable).ToList();
                variables.AddRange(calculusModel.Variables);

                var newModel = new CalculusModel(variables.Distinct().ToArray(), conditions);
                return (newModel, resultingBinders, true);
            }
            else if (changed)
            {
                return (calculusModel, resultingBinders, true);
            }
            else
            {
                return (calculusModel, valueBinders, false);
            }
        }

        private bool AlignIriValues(List<(IFilterCondition condition, BaseValueBinder valueBinder)> bindersToProcess, List<IAssignmentCondition> assignmentConditions, ref int caseIndex, List<SwitchValueBinder.Case> cases, List<CaseExpression.Statement> caseStatements, IQueryContext queryContext)
        {
            var matchGroups = new List<List<(IFilterCondition condition, BaseValueBinder valueBinder, Pattern templatePattern)>>();
            var shouldBeReplacedByExpression = GroupIriValueBinders(bindersToProcess, matchGroups);

            if (shouldBeReplacedByExpression)
            {
                throw new NotImplementedException();
            }
            else
            {
                bool modified = false;

                foreach (var matchGroup in matchGroups)
                {
                    if (matchGroup.Count == 1)
                    {
                        caseStatements.Add(new CaseExpression.Statement(matchGroup[0].condition, new ConstantExpression(caseIndex, queryContext)));
                        cases.Add(new SwitchValueBinder.Case(caseIndex++, matchGroup[0].valueBinder));
                    }
                    else
                    {
                        var firstValueBinder = matchGroup[0].valueBinder;

                        var similarities = matchGroup.Select(x =>
                        {
                            var isSimilar = ValueBindersSimilar(firstValueBinder, x.valueBinder, out var mapping);
                            return (isSimilar, x.condition, x.valueBinder, x.templatePattern, mapping);
                        }).ToList();

                        if (similarities.All(x => x.Item1))
                        {
                            modified = AlignSimilarIriValueBinders(assignmentConditions, ref caseIndex, cases, caseStatements, queryContext, similarities, firstValueBinder);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                return modified;
            }
        }

        private static bool AlignSimilarIriValueBinders(List<IAssignmentCondition> assignmentConditions, ref int caseIndex, List<SwitchValueBinder.Case> cases, List<CaseExpression.Statement> caseStatements, IQueryContext queryContext, List<(bool isSimilar, IFilterCondition condition, BaseValueBinder valueBinder, Pattern templatePattern, Dictionary<string, string> mapping)> matchGroup, BaseValueBinder firstValueBinder)
        {
            bool modified;
            if (firstValueBinder.TermMap.IsConstantValued)
            {
                var disjunctionCondition =
                    new DisjunctionCondition(matchGroup.Select(x => x.condition));

                caseStatements.Add(new CaseExpression.Statement(disjunctionCondition, new ConstantExpression(caseIndex, queryContext)));
                cases.Add(new SwitchValueBinder.Case(caseIndex++, firstValueBinder));
                modified = true;
            }
            else if (firstValueBinder.TermMap.IsTemplateValued)
            {
                var columnExpressions = new Dictionary<string, (AssignedVariable variable, List<(IFilterCondition condition, IExpression expression)> cases)>();

                foreach (var valueTuple in matchGroup)
                {
                    var secondValueBinder = valueTuple.valueBinder;

                    foreach (var templatePart in firstValueBinder.TemplateParts.Where(x => x.IsColumn))
                    {
                        var columnName = templatePart.Column;

                        if (!columnExpressions.TryGetValue(columnName, out var expressions))
                        {
                            expressions = (new AssignedVariable(queryContext.Db.SqlTypeForString), new List<(IFilterCondition condition, IExpression expression)>());
                            columnExpressions.Add(columnName, expressions);
                        }

                        if (!valueTuple.mapping.TryGetValue(columnName, out var secondColumnName))
                        {
                            secondColumnName = columnName;
                        }

                        expressions.cases.Add((valueTuple.condition, new ColumnExpression(secondValueBinder.GetCalculusVariable(secondColumnName), true)));
                    }
                }

                foreach (var columnExpression in columnExpressions.Values)
                {
                    assignmentConditions.Add(new AssignmentFromExpressionCondition(columnExpression.variable, new CaseExpression(columnExpression.cases.Select(x => new CaseExpression.Statement(x.condition, x.expression)))));
                }

                var newValueBinder = new BaseValueBinder(firstValueBinder, x => columnExpressions[x].variable);
                var disjunctionCondition =
                    new DisjunctionCondition(matchGroup.Select(x => x.condition));

                caseStatements.Add(new CaseExpression.Statement(disjunctionCondition, new ConstantExpression(caseIndex, queryContext)));
                cases.Add(new SwitchValueBinder.Case(caseIndex++, newValueBinder));
                modified = true;
            }
            else
            {
                throw new Exception("Should never happen as the column base term maps are handled differently");
            }

            return modified;
        }

        private bool GroupIriValueBinders(List<(IFilterCondition condition, BaseValueBinder valueBinder)> bindersToProcess, List<List<(IFilterCondition condition, BaseValueBinder valueBinder, Pattern templatePattern)>> matchGroups)
        {
            var shouldBeReplacedByExpression = false;

            foreach (var valueTuple in bindersToProcess)
            {
                var termMap = valueTuple.valueBinder.TermMap;
                var templateParts = new List<PatternItem>();

                if (termMap.IsColumnValued)
                {
                    shouldBeReplacedByExpression = true;
                    break;
                }
                else if (termMap.IsConstantValued)
                {
                    if(termMap.Iri != null)
                    {
                        templateParts.Add(new PatternItem(termMap.Iri.AbsoluteUri));
                    }
                    else
                    {
                        throw new Exception("Should never happen for IRI value binders");
                    }
                }
                else
                {
                    templateParts.AddRange(valueTuple.valueBinder.TemplateParts.Select(templatePart =>
                        PatternItem.FromTemplatePart(templatePart,
                            new Func<string, ICalculusVariable>(valueTuple.valueBinder.GetCalculusVariable))));
                }

                var templatePattern = new Pattern(true, templateParts);

                var foundGroup = false;

                foreach (var matchGroup in matchGroups)
                {
                    var isInGroup = matchGroup.Any(x => !_patternComparer.Compare(templatePattern, x.templatePattern).NeverMatch);

                    if (isInGroup)
                    {
                        matchGroup.Add((valueTuple.condition, valueTuple.valueBinder, templatePattern));
                        foundGroup = true;
                        break;
                    }
                }

                if (!foundGroup)
                {
                    matchGroups.Add(new List<(IFilterCondition condition, BaseValueBinder valueBinder, Pattern templatePattern)>()
                    {
                        (valueTuple.condition, valueTuple.valueBinder, templatePattern)
                    });
                }
            }

            return shouldBeReplacedByExpression;
        }

        private bool ValueBindersSimilar(BaseValueBinder firstValueBinder, BaseValueBinder secondValueBinder, out Dictionary<string, string> variableNames)
        {
            variableNames = new Dictionary<string, string>();

            if (firstValueBinder.TermMap == secondValueBinder.TermMap)
            {
                return true;
            }

            if (firstValueBinder.TermMap.IsTemplateValued && secondValueBinder.TermMap.IsTemplateValued)
            {
                var firstTemplateParts = firstValueBinder.TemplateParts.ToList();
                var secondTemplateParts = secondValueBinder.TemplateParts.ToList();

                if (firstTemplateParts.Count == secondTemplateParts.Count)
                {
                    bool same = true;

                    for (int i = 0; i < firstTemplateParts.Count; i++)
                    {
                        if (!firstTemplateParts[i].IsSimilarTo(secondTemplateParts[i], variableNames))
                        {
                            same = false;
                            break;
                        }
                    }

                    if (same)
                    {
                        return true;
                    }
                }
            }

            return false;
            // TODO: Add case when the term maps are not equal but they refer to the same constant
        }

        private int AlignLiteralValues(IValueBinder valueBinder, IValueType type, List<(IFilterCondition condition, BaseValueBinder)> typeGroup, List<IAssignmentCondition> assignmentConditions, int caseIndex, List<SwitchValueBinder.Case> cases, List<CaseExpression.Statement> caseStatements, IQueryContext queryContext, DataType columnType, Func<ExpressionsSet, IExpression> expressionSelector)
        {
            var newVariable = new AssignedVariable(columnType);
            var expressions = new List<(IFilterCondition condition, IExpression expression, IFilterCondition notAnErrorCondition)>();

            foreach (var valueTuple in typeGroup)
            {
                var expressionSet = _conditionBuilder.CreateExpression(queryContext, valueTuple.Item2);
                var expression = expressionSelector(expressionSet);
                expressions.Add((valueTuple.Item1, expression, expressionSet.IsNotErrorCondition));
            }

            var caseExpression = new CaseExpression(expressions.Select(x =>
                new CaseExpression.Statement(x.condition, x.expression)));

            assignmentConditions.Add(new AssignmentFromExpressionCondition(newVariable, caseExpression));

            var typeExpression = new ConstantExpression(queryContext.TypeCache.GetIndex(type),
                queryContext);
            var typeCategoryExpression = new ConstantExpression((int) type.Category, queryContext);
            var columnExpression = new ColumnExpression(newVariable, false);
            var notAnErrorCondition = new DisjunctionCondition(expressions.Select(x =>
                new ConjunctionCondition(new[] {x.condition, x.notAnErrorCondition})));

            ExpressionSetValueBinder newValueBinder = null;
            switch (type.Category)
            {
                case TypeCategories.BlankNode:
                case TypeCategories.SimpleLiteral:
                case TypeCategories.StringLiteral:
                case TypeCategories.OtherLiterals:
                    newValueBinder = new ExpressionSetValueBinder(valueBinder.VariableName,
                        new ExpressionsSet(notAnErrorCondition, typeExpression, typeCategoryExpression, columnExpression, null, null, null,
                            queryContext));
                    break;
                case TypeCategories.NumericLiteral:
                    newValueBinder = new ExpressionSetValueBinder(valueBinder.VariableName,
                        new ExpressionsSet(notAnErrorCondition, typeExpression, typeCategoryExpression, null, columnExpression, null, null,
                            queryContext));
                    break;
                case TypeCategories.BooleanLiteral:
                    newValueBinder = new ExpressionSetValueBinder(valueBinder.VariableName,
                        new ExpressionsSet(notAnErrorCondition, typeExpression, typeCategoryExpression, null, null, columnExpression, null,
                            queryContext));
                    break;
                case TypeCategories.DateTimeLiteral:
                    newValueBinder = new ExpressionSetValueBinder(valueBinder.VariableName,
                        new ExpressionsSet(notAnErrorCondition, typeExpression, typeCategoryExpression, null, null, null, columnExpression,
                            queryContext));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var condition = new DisjunctionCondition(typeGroup.Select(x => x.Item1));
            caseStatements.Add(new CaseExpression.Statement(condition, new ConstantExpression(caseIndex, queryContext)));
            cases.Add(new SwitchValueBinder.Case(caseIndex++, newValueBinder));
            return caseIndex;
        }

        private IValueBinder ConvertToExpressionSetValueBinder(IValueBinder valueBinder, IQueryContext queryContext)
        {
            // TODO: It should add the expressions to the SELECT statement
            throw new NotImplementedException();
        }
    }
}



