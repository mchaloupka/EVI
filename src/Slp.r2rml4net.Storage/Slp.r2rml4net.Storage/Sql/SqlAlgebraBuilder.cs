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
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Binders.Utils;
using TCode.r2rml4net.Mapping;
using Slp.r2rml4net.Storage.Mapping.Utils;
using VDS.RDF;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sql
{
    /// <summary>
    /// SQL algebra builder
    /// </summary>
    public class SqlAlgebraBuilder : ISparqlQueryVisitor
    {
        /// <summary>
        /// The condition builder
        /// </summary>
        private ConditionBuilder conditionBuilder;

        /// <summary>
        /// The template processor
        /// </summary>
        private TemplateProcessor templateProcessor;

        /// <summary>
        /// The expression builder
        /// </summary>
        private ExpressionBuilder expressionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlAlgebraBuilder"/> class.
        /// </summary>
        public SqlAlgebraBuilder()
        {
            this.expressionBuilder = new ExpressionBuilder();
            this.conditionBuilder = new ConditionBuilder(this.expressionBuilder);
            this.templateProcessor = new TemplateProcessor();
        }

        /// <summary>
        /// Processes the specified algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The context.</param>
        public INotSqlOriginalDbSource Process(ISparqlQuery algebra, QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, context);
        }

        /// <summary>
        /// Visits the specified BGP operator.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(BgpOp bgpOp, object data)
        {
            var context = (QueryContext)data;
            var source = ProcessBgpSource(bgpOp.R2RMLTripleDef, context);
            var select = new SqlSelectOp(source);

            ProcessBgpSubject(bgpOp, context, select, source);
            ProcessBgpPredicate(bgpOp, context, select, source);
            ProcessBgpObject(bgpOp, context, select, source);

            return context.OptimizeOnTheFly(select);
        }

        /// <summary>
        /// Visits the specified join operator.
        /// </summary>
        /// <param name="joinOp">The join operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

                    if (!current.CanBeMergedTo())
                        current = TransformToSelect(current, context);

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

        /// <summary>
        /// Visits the specified one empty solution operator.
        /// </summary>
        /// <param name="oneEmptySolutionOp">The one empty solution operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(OneEmptySolutionOp oneEmptySolutionOp, object data)
        {
            return new SingleEmptyRowSource();
        }

        /// <summary>
        /// Visits the specified union operator.
        /// </summary>
        /// <param name="unionOp">The union operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Gets the unioned column.
        /// </summary>
        /// <param name="sqlUnion">The SQL union.</param>
        /// <param name="neededColumn">The needed column.</param>
        /// <param name="context">The query context.</param>
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

        /// <summary>
        /// Visits the specified no solution operator.
        /// </summary>
        /// <param name="noSolutionOp">The no solution operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(NoSolutionOp noSolutionOp, object data)
        {
            return new NoRowSource();
        }

        /// <summary>
        /// Visits the specified slice operator.
        /// </summary>
        /// <param name="sliceOp">The slice operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified order by operator.
        /// </summary>
        /// <param name="orderByOp">The order by operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified distinct operator.
        /// </summary>
        /// <param name="distinctOp">The distinct operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified reduced operator.
        /// </summary>
        /// <param name="reducedOp">The reduced operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified bind operator.
        /// </summary>
        /// <param name="bindOp">The bind operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(BindOp bindOp, object data)
        {
            var context = (QueryContext)data;
            var inner = (INotSqlOriginalDbSource)bindOp.InnerQuery.Accept(this, context);

            var select = inner as SqlSelectOp;

            if (select == null)
                select = TransformToSelect(inner, context);

            var expression = expressionBuilder.ConvertExpression(bindOp.Expression, select.ValueBinders.ToList(), context);
            var valBinder = new ExpressionValueBinder(bindOp.VariableName, expression);

            select.AddValueBinder(valBinder);
            return select;
        }

        /// <summary>
        /// Visits the specified select operator.
        /// </summary>
        /// <param name="selectOp">The select operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
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

        /// <summary>
        /// Processes the BGP source.
        /// </summary>
        /// <param name="triplesMap">The triples map.</param>
        /// <param name="context">The query context.</param>
        /// <exception cref="System.Exception">Unknown source of bgp</exception>
        private ISqlOriginalDbSource ProcessBgpSource(ITriplesMap triplesMap, QueryContext context)
        {
            if (!string.IsNullOrEmpty(context.Mapping.Cache.GetSqlStatement(triplesMap)))
            {
                return new SqlStatement(context.Mapping.Cache.GetSqlStatement(triplesMap));
            }
            else if (!string.IsNullOrEmpty(context.Mapping.Cache.GetSqlTable(triplesMap)))
            {
                return new SqlTable(context.Mapping.Cache.GetSqlTable(triplesMap));
            }
            else
                throw new Exception("Unknown source of bgp");
        }

        /// <summary>
        /// Processes the BGP object.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="context">The query context.</param>
        /// <param name="select">The select.</param>
        /// <param name="childSource">The child source.</param>
        /// <exception cref="System.Exception">BgpOp must have object or ref object map</exception>
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

                var parentTriplesMap = refObjectPatern.GetParentTriplesMap(context.Mapping.Mapping);
                var parentSource = ProcessBgpSource(parentTriplesMap, context);

                List<ICondition> conditions = new List<ICondition>();

                foreach (var joinCond in refObjectPatern.GetJoinConditions())
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

        /// <summary>
        /// Processes the BGP predicate.
        /// </summary>
        /// <param name="bgpOp">The BGP op.</param>
        /// <param name="context">The context.</param>
        /// <param name="select">The select.</param>
        /// <param name="source">The source.</param>
        private void ProcessBgpPredicate(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source)
        {
            var predicatePattern = bgpOp.PredicatePattern;

            ProcessBgpPattern(bgpOp, context, select, source, predicatePattern, bgpOp.R2RMLPredicateMap);
        }

        /// <summary>
        /// Processes the BGP subject.
        /// </summary>
        /// <param name="bgpOp">The BGP op.</param>
        /// <param name="context">The context.</param>
        /// <param name="select">The select.</param>
        /// <param name="source">The source.</param>
        private void ProcessBgpSubject(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source)
        {
            var subjectPattern = bgpOp.SubjectPattern;

            ProcessBgpPattern(bgpOp, context, select, source, subjectPattern, bgpOp.R2RMLSubjectMap);
        }

        /// <summary>
        /// Processes the BGP pattern.
        /// </summary>
        /// <param name="bgpOp">The BGP op.</param>
        /// <param name="context">The context.</param>
        /// <param name="select">The select.</param>
        /// <param name="source">The source.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="r2rmlMap">The R2RML map.</param>
        /// <exception cref="System.NotImplementedException"></exception>
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

        /// <summary>
        /// Processes the BGP condition.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="context">The context.</param>
        /// <param name="select">The select.</param>
        /// <param name="source">The source.</param>
        /// <param name="node">The node.</param>
        /// <param name="r2rmlMap">The R2RML map.</param>
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

        /// <summary>
        /// Processes the BGP variable.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="context">The query context.</param>
        /// <param name="select">The select.</param>
        /// <param name="source">The source.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="r2rmlMap">The R2RML map.</param>
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

        /// <summary>
        /// Transforms to select.
        /// </summary>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="context">The query context.</param>
        private SqlSelectOp TransformToSelect(INotSqlOriginalDbSource sqlQuery, QueryContext context)
        {
            var select = new SqlSelectOp(sqlQuery);

            foreach (var valueBinder in sqlQuery.ValueBinders)
            {
                select.AddValueBinder(valueBinder.GetSelectValueBinder(select, context));
            }

            return select;
        }

        /// <summary>
        /// Processes the join.
        /// </summary>
        /// <param name="first">The first join.</param>
        /// <param name="second">The second join.</param>
        /// <param name="context">The query context.</param>
        private void ProcessJoin(SqlSelectOp first, INotSqlOriginalDbSource second, QueryContext context)
        {
            if (!(second is SqlSelectOp) || !((SqlSelectOp)second).IsMergeableTo(first))
                second = TransformToSelect(second, context);

            ProcessJoin(first, (SqlSelectOp)second, context);
        }

        /// <summary>
        /// Processes the join.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="context">The context.</param>
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
