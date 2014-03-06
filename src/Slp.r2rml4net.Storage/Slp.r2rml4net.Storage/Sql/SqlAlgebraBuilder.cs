using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Storage.Sql.Binders;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sql
{
    public class SqlAlgebraBuilder
    {
        private ConditionBuilder conditionBuilder;
        private TemplateProcessor templateProcessor;
        private ExpressionBuilder expressionBuilder;

        public SqlAlgebraBuilder()
        {
            this.expressionBuilder = new ExpressionBuilder();
            this.conditionBuilder = new ConditionBuilder(this.expressionBuilder);
            this.templateProcessor = new TemplateProcessor();
        }

        public INotSqlOriginalDbSource Process(ISparqlQuery query, QueryContext context)
        {
            if (query is NoSolutionOp)
            {
                return new NoRowSource();
            }
            else if (query is OneEmptySolutionOp)
            {
                return new SingleEmptyRowSource();
            }
            else if (query is SelectOp)
            {
                return ProcessSelect((SelectOp)query, context);
            }
            else if (query is BgpOp)
            {
                return ProcessBgp((BgpOp)query, context);
            }
            else if (query is JoinOp)
            {
                return ProcessJoin((JoinOp)query, context);
            }
            else if (query is UnionOp)
            {
                return ProcessUnion((UnionOp)query, context);
            }

            // TODO: Process others

            throw new Exception("Cannot handle unknown sparql algebra type");
        }

        private INotSqlOriginalDbSource ProcessBgp(BgpOp bgpOp, QueryContext context)
        {
            var source = ProcessBgpSource(bgpOp.R2RMLTripleDef);
            var select = new SqlSelectOp(source);

            ProcessBgpSubject(bgpOp, context, select, source);
            ProcessBgpPredicate(bgpOp, context, select, source);
            ProcessBgpObject(bgpOp, context, select, source);

            return select;
        }

        private ISqlOriginalDbSource ProcessBgpSource(ITriplesMap triplesMap)
        {
            if (!string.IsNullOrEmpty(triplesMap.SqlQuery))
            {
                return new SqlStatement(triplesMap.SqlQuery);
            }
            else if (!string.IsNullOrEmpty(triplesMap.TableName))
            {
                return new SqlTable(triplesMap.TableName);
            }
            else
                throw new Exception("Unknown source of bgp");
        }

        private void ProcessBgpObject(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource childSource)
        {
            if (bgpOp.R2RMLObjectMap != null)
            {
                var objectPattern = bgpOp.ObjectPattern;

                ProcessBgpPattern(bgpOp, context, select, childSource, objectPattern, bgpOp.R2RMLObjectMap);
            }
            else if (bgpOp.R2RMLRefObjectMap != null)
            {
                var refObjectPatern = bgpOp.R2RMLRefObjectMap;
                var parentTriplesMap = GetParentTriplesMap(context, refObjectPatern);

                var parentSource = ProcessBgpSource(parentTriplesMap);
                List<ICondition> conditions = new List<ICondition>();

                foreach (var joinCond in refObjectPatern.JoinConditions)
                {
                    var childCol = childSource.GetColumn(joinCond.ChildColumn);
                    var parentCol = parentSource.GetColumn(joinCond.ParentColumn);

                    conditions.Add(conditionBuilder.CreateEqualsCondition(context, childCol, parentCol));
                }

                ICondition joinCondition = null;
                if (conditions.Count == 0)
                {
                    joinCondition = conditionBuilder.CreateAlwaysTrueCondition(context);
                }
                else if (conditions.Count == 1)
                {
                    joinCondition = conditions[0];
                }
                else
                {
                    joinCondition = conditionBuilder.CreateAndCondition(context, conditions);
                }

                select.AddJoinedSource(parentSource, joinCondition, context);

                ProcessBgpPattern(bgpOp, context, select, parentSource, bgpOp.ObjectPattern, refObjectPatern.SubjectMap);
            }
            else throw new Exception("BgpOp must have object or ref object map");
        }

        private ITriplesMap GetParentTriplesMap(QueryContext context, IRefObjectMap refObjectPatern)
        {
            // TODO: Remove this method as soon as the reference will be public

            var subjectMap = refObjectPatern.SubjectMap;

            foreach (var tripleMap in context.Mapping.Mapping.TriplesMaps)
            {
                if (tripleMap.SubjectMap == subjectMap)
                    return tripleMap;
            }

            throw new Exception("Triples map not found");
        }

        private void ProcessBgpPredicate(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source)
        {
            var predicatePattern = bgpOp.PredicatePattern;

            ProcessBgpPattern(bgpOp, context, select, source, predicatePattern, bgpOp.R2RMLPredicateMap);
        }

        private void ProcessBgpSubject(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source)
        {
            var subjectPattern = bgpOp.SubjectPattern;

            ProcessBgpPattern(bgpOp, context, select, source, subjectPattern, bgpOp.R2RMLSubjectMap);
        }

        private void ProcessBgpPattern(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source, PatternItem pattern, ITermMap r2rmlMap)
        {
            if (pattern is VariablePattern)
            {
                ProcessBgpVariable(bgpOp, context, select, source, pattern.VariableName, r2rmlMap);
            }
            else if (pattern is NodeMatchPattern)
            {
                var node = ((NodeMatchPattern)pattern).Node;

                ProcessBgpCondition(bgpOp, context, select, source, node, r2rmlMap);
            }
            else
                throw new NotImplementedException();

            // TODO: Condition for not variable and node match patterns
            // http://dotnetrdf.org/API/dotNetRDF~VDS.RDF.Query.Patterns.PatternItem.html
        }

        private void ProcessBgpCondition(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source, INode node, ITermMap r2rmlMap)
        {
            var valueBinder = new ValueBinder(r2rmlMap, templateProcessor);

            foreach (var column in valueBinder.NeededColumns)
            {
                var sqlColumn = source.GetColumn(column);
                valueBinder.SetColumn(column, sqlColumn);
            }

            var condition = conditionBuilder.CreateEqualsCondition(context, node, valueBinder);

            select.AddCondition(condition);
        }

        private void ProcessBgpVariable(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source, string variableName, ITermMap r2rmlMap)
        {
            var valueBinder = new ValueBinder(variableName, r2rmlMap, templateProcessor);

            foreach (var column in valueBinder.NeededColumns)
            {
                var sourceColumn = source.GetColumn(column);
                var sqlColumn = select.GetSelectColumn(sourceColumn);
                valueBinder.SetColumn(column, sqlColumn);

                var condition = conditionBuilder.CreateIsNotNullCondition(context, sourceColumn);
                select.AddCondition(condition);
            }

            select.AddValueBinder(valueBinder);
        }

        private INotSqlOriginalDbSource ProcessSelect(SelectOp selectOp, QueryContext context)
        {
            var inner = Process(selectOp.InnerQuery, context);

            if (selectOp.IsSelectAll)
            {
                return inner;
            }

            var newVariables = selectOp.Variables;
            var valueBinders = new List<IBaseValueBinder>();

            foreach (var variable in newVariables)
            {
                if (variable.IsProjection)
                {
                    throw new NotImplementedException();
                }
                else if (variable.IsAggregate)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var valBinder = inner.ValueBinders.Where(x => x.VariableName == variable.Name).FirstOrDefault();

                    if (valBinder == null)
                    {
                        throw new Exception("Can't find variable in inner query (sql select operator)");
                    }
                    else
                    {
                        valueBinders.Add(valBinder);
                    }
                }
            }

            var neededColumns = valueBinders.SelectMany(x => x.AssignedColumns).Distinct().ToArray();

            SqlSelectOp select = inner as SqlSelectOp;

            if (select == null)
            {
                select = TransformToSelect(inner, context);
            }

            var notNeeded = select.Columns.Where(x => !neededColumns.Contains(x)).ToArray();

            foreach (var col in notNeeded)
            {
                select.RemoveColumn(col);
            }

            var valBindersToRemove = inner.ValueBinders.Where(x => !valueBinders.Contains(x)).ToArray();
            foreach (var valBinder in valBindersToRemove)
            {
                inner.RemoveValueBinder(valBinder);
            }
            return inner;
        }

        private INotSqlOriginalDbSource ProcessJoin(JoinOp joinOp, QueryContext context)
        {
            var joinedProcessed = joinOp.GetInnerQueries().Select(x => Process(x, context)).ToArray();

            if (joinedProcessed.Length == 0)
            {
                return new SingleEmptyRowSource();
            }
            else
            {
                var selects = joinedProcessed.OfType<SqlSelectOp>();
                var notSelects = joinedProcessed.Where(x => !(x is SqlSelectOp));

                SqlSelectOp current = null;

                if (!selects.Any())
                {
                    current = TransformToSelect(notSelects.First(), context);
                    notSelects = notSelects.Skip(1);
                }
                else
                {
                    current = selects.First();

                    foreach (var select in selects.Skip(1))
                    {
                        ProcessJoin(current, select, context);
                    }
                }

                foreach (var notSelect in notSelects)
                {
                    ProcessJoin(current, notSelect, context);
                }

                return current;
            }
        }

        private SqlSelectOp TransformToSelect(INotSqlOriginalDbSource sqlQuery, QueryContext context)
        {
            if (sqlQuery is SqlSelectOp)
                return (SqlSelectOp)sqlQuery;

            var select = new SqlSelectOp(sqlQuery);

            foreach (var valueBinder in sqlQuery.ValueBinders)
            {
                select.AddValueBinder(GetSelectValueBinder(valueBinder, select, context));
            }

            return select;
        }

        private void ProcessJoin(SqlSelectOp first, INotSqlOriginalDbSource second, QueryContext context)
        {
            // TODO: Rework the "have only original source and conditions"

            if (!(second is SqlSelectOp))
                second = TransformToSelect(second, context);

            if (((SqlSelectOp)second).HaveOnlyOriginalSourceAndConditions)
                ProcessJoin(first, (SqlSelectOp)second, context);
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ProcessJoin(SqlSelectOp first, SqlSelectOp second, QueryContext context)
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var firstValBinder in first.ValueBinders)
            {
                foreach (var secondValBinder in second.ValueBinders)
                {
                    if (firstValBinder.VariableName == secondValBinder.VariableName)
                    {
                        conditions.Add(conditionBuilder.CreateJoinEqualsCondition(context, GetOriginalValueBinder(firstValBinder, context), GetOriginalValueBinder(secondValBinder, context)));
                    }
                }
            }

            ICondition condition = conditionBuilder.CreateAlwaysTrueCondition(context);

            if (conditions.Count == 1)
            {
                condition = conditions[0];
            }
            else if (conditions.Count > 1)
            {
                condition = conditionBuilder.CreateAndCondition(context, conditions);
            }

            first.AddJoinedSource(second.OriginalSource, condition, context);

            foreach (var cond in second.Conditions)
            {
                first.AddCondition(cond);
            }

            var firstValueBinders = first.ValueBinders.ToArray();
            var secondValueBinders = second.ValueBinders.ToArray();

            foreach (var fBinder in firstValueBinders)
            {
                var sBinder = secondValueBinders.Where(x => x.VariableName == fBinder.VariableName).FirstOrDefault();

                if (sBinder != null)
                {
                    var origSBinder = GetOriginalValueBinder(sBinder, context);
                    var selSBinder = GetSelectValueBinder(origSBinder, first, context);

                    var newBinder = new CollateValueBinder(fBinder);
                    newBinder.AddValueBinder(selSBinder);

                    first.ReplaceValueBinder(fBinder, newBinder);
                }
            }

            foreach (var sBinder in secondValueBinders)
            {
                var fBinder = firstValueBinders.Where(x => x.VariableName == sBinder.VariableName).FirstOrDefault();

                if (fBinder == null)
                {
                    var origSBinder = GetOriginalValueBinder(sBinder, context);
                    var selSBinder = GetSelectValueBinder(origSBinder, first, context);
                    first.AddValueBinder(selSBinder);
                }
            }
        }

        private IBaseValueBinder GetSelectValueBinder(IBaseValueBinder binder, SqlSelectOp first, QueryContext context)
        {
            var newBinder = (IBaseValueBinder)binder.Clone();

            foreach (var col in newBinder.AssignedColumns.ToArray())
            {
                newBinder.ReplaceAssignedColumn(col, first.GetSelectColumn(col));
            }

            return newBinder;
        }

        private IBaseValueBinder GetOriginalValueBinder(IBaseValueBinder binder, QueryContext context)
        {
            var newBinder = (IBaseValueBinder)binder.Clone();

            foreach (var oldColumn in newBinder.AssignedColumns.ToArray())
            {
                if (!(oldColumn is SqlSelectColumn))
                {
                    throw new Exception("Can't get original value binder if it is not from sql select columns");
                }

                newBinder.ReplaceAssignedColumn(oldColumn, ((SqlSelectColumn)oldColumn).OriginalColumn);
            }

            return newBinder;
        }

        private INotSqlOriginalDbSource ProcessUnion(UnionOp unionOp, QueryContext context)
        {
            var unioned = unionOp.GetInnerQueries().Select(x => Process(x, context));

            List<SqlSelectOp> selects = new List<SqlSelectOp>();

            foreach (var unionSource in unioned)
            {
                if (unionSource is SqlSelectOp)
                {
                    selects.Add((SqlSelectOp)unionSource);
                }
                else
                {
                    selects.Add(TransformToSelect(unionSource, context));
                }
            }

            var variableNames = selects.SelectMany(x => x.ValueBinders).Select(x => x.VariableName).Distinct().ToArray();

            Dictionary<string, CaseValueBinder> valueBinders = new Dictionary<string, CaseValueBinder>();

            foreach (var varName in variableNames)
            {
                valueBinders.Add(varName, new CaseValueBinder(varName));
            }

            var sqlUnion = new SqlUnionOp();

            for (int index = 0; index < selects.Count; index++)
            {
                Dictionary<ISqlColumn, SqlUnionColumn> unColumns = new Dictionary<ISqlColumn, SqlUnionColumn>();

                var select = selects[index];
                sqlUnion.AddSource(select);

                var condExpr = expressionBuilder.CreateExpression(context, index);
                var column = select.GetExpressionColumn(condExpr);
                sqlUnion.CaseColumn.AddColumn(column);
                var cond = conditionBuilder.CreateEqualsCondition(context, sqlUnion.CaseColumn, condExpr);

                foreach (var valBinder in select.ValueBinders)
                {
                    var caseValBinder = valueBinders[valBinder.VariableName];
                    var cloned = (IBaseValueBinder)valBinder.Clone();

                    foreach (var neededColumn in cloned.AssignedColumns.ToArray())
                    {
                        if (!unColumns.ContainsKey(neededColumn))
                        {
                            var newCol = sqlUnion.GetUnionedColumn();
                            unColumns.Add(neededColumn, newCol);
                            newCol.AddColumn(neededColumn);
                        }

                        var unColumn = unColumns[neededColumn];
                        cloned.ReplaceAssignedColumn(neededColumn, unColumn);
                    }

                    caseValBinder.AddValueBinder((ICondition)cond.Clone(), cloned);
                }
            }

            foreach (var valBinder in valueBinders.Select(x => x.Value))
            {
                sqlUnion.AddValueBinder(valBinder);
            }

            return sqlUnion;
        }
    }
}
