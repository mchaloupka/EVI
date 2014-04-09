using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Binders.Utils;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sql
{
    public class SqlAlgebraBuilder : ISparqlQueryVisitor
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

        public INotSqlOriginalDbSource Process(ISparqlQuery algebra, QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, context);
        }

        public object Visit(BgpOp bgpOp, object data)
        {
            var context = (QueryContext)data;
            var source = ProcessBgpSource(bgpOp.R2RMLTripleDef);
            var select = new SqlSelectOp(source);

            ProcessBgpSubject(bgpOp, (QueryContext)data, select, source);
            ProcessBgpPredicate(bgpOp, (QueryContext)data, select, source);
            ProcessBgpObject(bgpOp, (QueryContext)data, select, source);

            return context.OptimizeOnTheFly(select);
        }

        public object Visit(JoinOp joinOp, object data)
        {
            var context = (QueryContext)data;
            var joinedProcessed = joinOp.GetInnerQueries().Select(x => x.Accept(this, data)).OfType<INotSqlOriginalDbSource>().ToArray();

            if (joinedProcessed.Length == 0)
            {
                return new SingleEmptyRowSource();
            }
            else
            {
                var selects = joinedProcessed.Where(x => x is SqlSelectOp);
                var notSelects = joinedProcessed.Where(x => !(x is SqlSelectOp));

                SqlSelectOp current = null;

                if (!selects.Any())
                {
                    current = TransformToSelect(notSelects.First(), context);
                    notSelects = notSelects.Skip(1);
                }
                else
                {
                    current = (SqlSelectOp)selects.First();

                    foreach (var select in selects.Skip(1))
                    {
                        ProcessJoin(current, select, context);
                    }
                }

                foreach (var notSelect in notSelects)
                {
                    ProcessJoin(current, notSelect, context);
                }

                return context.OptimizeOnTheFly(current);
            }
        }

        public object Visit(OneEmptySolutionOp oneEmptySolutionOp, object data)
        {
            // TODO: Implement this
            throw new NotImplementedException();
        }

        public object Visit(UnionOp unionOp, object data)
        {
            var context = (QueryContext)data;
            var unioned = unionOp.GetInnerQueries().Select(x => x.Accept(this, context)).OfType<INotSqlOriginalDbSource>();

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
                            var newCol = GetUnionedColumn(sqlUnion, neededColumn, (QueryContext)data);
                            unColumns.Add(neededColumn, newCol);
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

            return context.OptimizeOnTheFly(sqlUnion);
        }

        private static SqlUnionColumn GetUnionedColumn(SqlUnionOp sqlUnion, ISqlColumn neededColumn, QueryContext context)
        {
            SqlUnionColumn newCol = null;

            foreach (var column in sqlUnion.Columns.OfType<SqlUnionColumn>())
            {
                var sources = column.OriginalColumns.Select(x => x.Source);

                if (sources.Contains(neededColumn.Source))
                    continue;
                else
                {
                    if (column.OriginalColumns.Any(x => !context.Db.CanBeUnioned(x, neededColumn)))
                        continue;

                    newCol = column;
                    break;
                }
            }

            if (newCol == null)
                newCol = sqlUnion.GetUnionedColumn();

            newCol.AddColumn(neededColumn);

            return newCol;
        }

        public object Visit(NoSolutionOp noSolutionOp, object data)
        {
            return new NoRowSource();
        }

        public object Visit(SliceOp sliceOp, object data)
        {
            var context = (QueryContext)data;
            var inner = (INotSqlOriginalDbSource)sliceOp.InnerQuery.Accept(this, context);

            var select = inner as SqlSelectOp;

            if (select == null)
                select = TransformToSelect(inner, context);

            if (select.Limit.HasValue && sliceOp.Limit.HasValue)
            {
                select.Limit = Math.Min(select.Limit.Value, sliceOp.Limit.Value);
            }
            else if (sliceOp.Limit.HasValue)
            {
                select.Limit = sliceOp.Limit.Value;
            }

            if (select.Offset.HasValue && sliceOp.Offset.HasValue)
            {
                select.Offset = Math.Max(select.Offset.Value, sliceOp.Offset.Value);
            }
            else if (sliceOp.Offset.HasValue)
            {
                select.Offset = sliceOp.Offset.Value;
            }

            return select;
        }

        public object Visit(OrderByOp orderByOp, object data)
        {
            var context = (QueryContext)data;
            var inner = (INotSqlOriginalDbSource)orderByOp.InnerQuery.Accept(this, context);

            var select = inner as SqlSelectOp;

            if (select == null)
                select = TransformToSelect(inner, context);

            foreach (var ordering in orderByOp.Orderings.Reverse())
            {
                var expression = expressionBuilder.CreateOrderByExpression(ordering.Expression, select, context);
                select.InsertOrdering(expression, ordering.Descending);
            }

            return select;
        }

        public object Visit(DistinctOp distinctOp, object data)
        {
            var context = (QueryContext)data;
            var inner = (INotSqlOriginalDbSource)distinctOp.InnerQuery.Accept(this, context);

            var select = inner as SqlSelectOp;

            if (select == null)
                select = TransformToSelect(inner, context);

            select.IsDistinct = true;

            foreach (var binder in select.ValueBinders.ToArray())
            {
                if (binder is ValueBinder || binder is SqlSideValueBinder)
                    continue;

                var expression = expressionBuilder.CreateExpressionForSqlSideValueBinder(binder, context);

                var newBinder = new SqlSideValueBinder(select.GetExpressionColumn(expression), binder);
                select.ReplaceValueBinder(binder, newBinder);
            }

            var neededColumns = select.ValueBinders.SelectMany(x => x.AssignedColumns).Distinct().ToArray();
            var notNeededColumns = select.Columns.Where(x => !neededColumns.Contains(x)).ToArray();

            foreach (var col in notNeededColumns)
            {
                select.RemoveColumn(col);
            }

            return select;
        }

        public object Visit(ReducedOp reducedOp, object data)
        {
            var context = (QueryContext)data;
            var inner = (INotSqlOriginalDbSource)reducedOp.InnerQuery.Accept(this, context);

            var select = inner as SqlSelectOp;

            if (select == null)
                select = TransformToSelect(inner, context);

            select.IsDistinct = true;

            return select;
        }

        public object Visit(SelectOp selectOp, object data)
        {
            var context = (QueryContext)data;
            var inner = (INotSqlOriginalDbSource)selectOp.InnerQuery.Accept(this, context);

            SqlSelectOp select = inner as SqlSelectOp;

            if (select == null)
            {
                select = TransformToSelect(inner, context);
            }

            var valueBinders = new List<IBaseValueBinder>();

            if (selectOp.IsSelectAll)
            {
                foreach (var valBinder in select.ValueBinders)
                {
                    if (valBinder.VariableName.StartsWith("_:")) // blank node match
                        continue;
                    else
                        valueBinders.Add(valBinder);
                }
            }
            else
            {
                var newVariables = selectOp.Variables;

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
                        var valBinder = select.ValueBinders.Where(x => x.VariableName == variable.Name).FirstOrDefault();

                        if (valBinder != null)
                        {
                            valueBinders.Add(valBinder);
                        }
                    }
                }
            }

            var neededColumns = valueBinders.SelectMany(x => x.AssignedColumns).Distinct().ToArray();
            var notNeeded = select.Columns.Where(x => !neededColumns.Contains(x)).ToArray();

            foreach (var col in notNeeded)
            {
                select.RemoveColumn(col);
            }

            var valBindersToRemove = select.ValueBinders.Where(x => !valueBinders.Contains(x)).ToArray();
            foreach (var valBinder in valBindersToRemove)
            {
                select.RemoveValueBinder(valBinder);
            }

            return context.OptimizeOnTheFly(select);
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
            else if (pattern is BlankNodePattern)
            {
                ProcessBgpVariable(bgpOp, context, select, source, ((BlankNodePattern)pattern).ID, r2rmlMap);
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

            var sameVarValueBinder = select.ValueBinders.Where(x => x.VariableName == variableName).FirstOrDefault();

            if(sameVarValueBinder == null)
                select.AddValueBinder(valueBinder);
            else
            {
                var condition = conditionBuilder.CreateEqualsCondition(context, sameVarValueBinder.GetOriginalValueBinder(context), valueBinder.GetOriginalValueBinder(context));
                select.AddCondition(condition);
            }
        }

        private SqlSelectOp TransformToSelect(INotSqlOriginalDbSource sqlQuery, QueryContext context)
        {
            var select = new SqlSelectOp(sqlQuery);

            foreach (var valueBinder in sqlQuery.ValueBinders)
            {
                select.AddValueBinder(valueBinder.GetSelectValueBinder(select, context));
            }

            return select;
        }

        private void ProcessJoin(SqlSelectOp first, INotSqlOriginalDbSource second, QueryContext context)
        {
            if (!(second is SqlSelectOp) || !((SqlSelectOp)second).IsMergeable)
                second = TransformToSelect(second, context);

            ProcessJoin(first, (SqlSelectOp)second, context);
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
                        conditions.Add(conditionBuilder.CreateJoinEqualsCondition(context, firstValBinder.GetOriginalValueBinder(context), secondValBinder.GetOriginalValueBinder(context)));
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

            foreach (var secondJoin in second.JoinSources)
            {
                first.AddJoinedSource(secondJoin.Source, secondJoin.Condition, context);
            }

            foreach (var secondJoin in second.LeftOuterJoinSources)
            {
                first.AddLeftOuterJoinedSource(secondJoin.Source, secondJoin.Condition, context);
            }

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
                    var origSBinder = sBinder.GetOriginalValueBinder(context);
                    var selSBinder = origSBinder.GetSelectValueBinder(first, context);

                    var newBinder = new CoalesceValueBinder(fBinder);
                    newBinder.AddValueBinder(selSBinder);

                    first.ReplaceValueBinder(fBinder, newBinder);
                }
            }

            foreach (var sBinder in secondValueBinders)
            {
                var fBinder = firstValueBinders.Where(x => x.VariableName == sBinder.VariableName).FirstOrDefault();

                if (fBinder == null)
                {
                    var origSBinder = sBinder.GetOriginalValueBinder(context);
                    var selSBinder = origSBinder.GetSelectValueBinder(first, context);
                    first.AddValueBinder(selSBinder);
                }
            }
        }
    }
}
