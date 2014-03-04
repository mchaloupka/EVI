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

        public SqlAlgebraBuilder()
        {
            this.conditionBuilder = new ConditionBuilder();
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
            ISqlOriginalDbSource source = null;

            if (!string.IsNullOrEmpty(bgpOp.R2RMLTripleDef.SqlQuery))
            {
                source = new SqlStatement(bgpOp.R2RMLTripleDef.SqlQuery);
            }
            else if (!string.IsNullOrEmpty(bgpOp.R2RMLTripleDef.TableName))
            {
                source = new SqlTable(bgpOp.R2RMLTripleDef.TableName);
            }

            SqlSelectOp select = new SqlSelectOp(source);

            ProcessBgpSubject(bgpOp, context, select, source);
            ProcessBgpPredicate(bgpOp, context, select, source);
            ProcessBgpObject(bgpOp, context, select, source);

            return select;
        }

        private void ProcessBgpObject(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source)
        {
            if (bgpOp.R2RMLObjectMap != null)
            {
                var objectPattern = bgpOp.ObjectPattern;

                if (objectPattern is VariablePattern)
                {
                    ProcessBgpVariable(bgpOp, context, select, source, objectPattern.VariableName, bgpOp.R2RMLObjectMap);
                }
                else if (objectPattern is NodeMatchPattern)
                {
                    var node = ((NodeMatchPattern)objectPattern).Node;

                    ProcessBgpCondition(bgpOp, context, select, source, node, bgpOp.R2RMLObjectMap);
                }
                // TODO: Condition for not variable and node match patterns
                // http://dotnetrdf.org/API/dotNetRDF~VDS.RDF.Query.Patterns.PatternItem.html
            }
            else if (bgpOp.R2RMLRefObjectMap != null)
            {
                throw new NotImplementedException();
            }
            else throw new Exception("BgpOp must have object or ref object map");
        }

        private void ProcessBgpPredicate(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source)
        {
            var predicatePattern = bgpOp.PredicatePattern;

            if (predicatePattern is VariablePattern)
            {
                ProcessBgpVariable(bgpOp, context, select, source, predicatePattern.VariableName, bgpOp.R2RMLPredicateMap);
            }
            else if (predicatePattern is NodeMatchPattern)
            {
                var node = ((NodeMatchPattern)predicatePattern).Node;

                ProcessBgpCondition(bgpOp, context, select, source, node, bgpOp.R2RMLPredicateMap);
            }
            // TODO: Condition for not variable and node match patterns
            // http://dotnetrdf.org/API/dotNetRDF~VDS.RDF.Query.Patterns.PatternItem.html
        }

        private void ProcessBgpSubject(BgpOp bgpOp, QueryContext context, SqlSelectOp select, ISqlOriginalDbSource source)
        {
            // TODO: Merge with predicate

            var subjectPattern = bgpOp.SubjectPattern;

            if (subjectPattern is VariablePattern)
            {
                ProcessBgpVariable(bgpOp, context, select, source, subjectPattern.VariableName, bgpOp.R2RMLSubjectMap);
            }
            else if (subjectPattern is NodeMatchPattern)
            {
                var node = ((NodeMatchPattern)subjectPattern).Node;

                ProcessBgpCondition(bgpOp, context, select, source, node, bgpOp.R2RMLSubjectMap);
            }
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

            if (inner is SqlSelectOp)
            {
                var select = (SqlSelectOp)inner;

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
            else
            {
                throw new NotImplementedException();
            }
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
                    var firstNotSelect = TransformToSelect(notSelects.First(), context);
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

        private SqlSelectOp TransformToSelect(ISqlSource sqlQuery, QueryContext context)
        {
            throw new NotImplementedException();
        }

        private void ProcessJoin(SqlSelectOp first, INotSqlOriginalDbSource second, QueryContext context)
        {
            if (second is SqlSelectOp && ((SqlSelectOp)second).HaveOnlyOriginalSourceAndConditions)
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
            if (binder is ValueBinder)
            {
                var oldBinder = (ValueBinder)binder;
                var newBinder = new ValueBinder(oldBinder.VariableName, oldBinder.R2RMLMap, oldBinder.TemplateProcessor);

                foreach (var colName in newBinder.NeededColumns)
                {
                    var oldColumn = oldBinder.GetColumn(colName);
                    newBinder.SetColumn(colName, first.GetSelectColumn(oldColumn));
                }

                return newBinder;
            }
            else if (binder is CollateValueBinder)
            {
                var oldBinder = (CollateValueBinder)binder;
                var newSubBinders = oldBinder.InnerBinders.Select(x => GetSelectValueBinder(x, first, context));
                var firstNew = newSubBinders.First();
                newSubBinders = newSubBinders.Skip(1);

                var newBinder = new CollateValueBinder(firstNew);

                foreach (var b in newSubBinders)
                {
                    newBinder.AddValueBinder(newBinder);
                }

                return newBinder;
            }
            else
                throw new Exception("Value binder can be only standard or collate");
        }

        private IBaseValueBinder GetOriginalValueBinder(IBaseValueBinder binder, QueryContext context)
        {
            if (binder is ValueBinder)
            {
                var oldBinder = (ValueBinder)binder;
                var newBinder = new ValueBinder(oldBinder.VariableName, oldBinder.R2RMLMap, oldBinder.TemplateProcessor);

                foreach (var colName in newBinder.NeededColumns)
                {
                    var oldColumn = oldBinder.GetColumn(colName);

                    if (!(oldColumn is SqlSelectColumn))
                    {
                        throw new Exception("Can't get original value binder if it is not from sql select columns");
                    }

                    newBinder.SetColumn(colName, ((SqlSelectColumn)oldColumn).OriginalColumn);
                }

                return newBinder;
            }
            else if (binder is CollateValueBinder)
            {
                var oldBinder = (CollateValueBinder)binder;
                var newSubBinders = oldBinder.InnerBinders.Select(x => GetOriginalValueBinder(x, context));
                var firstNew = newSubBinders.First();
                newSubBinders = newSubBinders.Skip(1);

                var newBinder = new CollateValueBinder(firstNew);

                foreach (var b in newSubBinders)
                {
                    newBinder.AddValueBinder(newBinder);
                }

                return newBinder;
            }
            else
                throw new Exception("Unknown value binder");
        }

        private INotSqlOriginalDbSource ProcessUnion(UnionOp unionOp, QueryContext context)
        {
            var unioned = unionOp.GetInnerQueries().Select(x => Process(x, context));

            List<SqlSelectOp> selects = new List<SqlSelectOp>();

            foreach (var unionSource in selects)
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

            throw new NotImplementedException();
        }
    }
}
