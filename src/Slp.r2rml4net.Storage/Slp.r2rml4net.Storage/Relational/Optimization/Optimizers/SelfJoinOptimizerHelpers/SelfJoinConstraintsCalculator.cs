using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Query.Sources;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// Calculator of self join constraints, able to find self join of SqlTables according to filter conditions
    /// </summary>
    public class SelfJoinConstraintsCalculator
    {
        /// <summary>
        /// Processes the self join conditions.
        /// </summary>
        /// <param name="filterConditions">The filter conditions.</param>
        /// <param name="presentTables"></param>
        /// <param name="data">The data.</param>
        /// <returns>List of all tables that are self joined</returns>
        public Dictionary<SqlTable, SqlTable> ProcessSelfJoinConditions(IEnumerable<IFilterCondition> filterConditions, List<SqlTable> presentTables, BaseRelationalOptimizer<SelfJoinOptimizerData>.OptimizationContext data)
        {
            var result = new Dictionary<SqlTable, SqlTable>();

            var satisfactionMap = CreateInitialSatisfactionMap(presentTables, data.Context);
            var filterConditionArray = filterConditions.ToArray();

            foreach (var filterCondition in filterConditionArray.OfType<EqualVariablesCondition>())
            {
                var leftVariable = filterCondition.LeftVariable as SqlColumn;
                var rightVariable = filterCondition.RightVariable as SqlColumn;

                if (leftVariable != null && rightVariable != null)
                {
                    var satisfaction = GetSatisfactionFromMap(satisfactionMap, leftVariable.Table, rightVariable.Table);

                    if (satisfaction != null && !result.ContainsKey(satisfaction.SqlTable) && !satisfaction.IsSatisfied)
                    {
                        satisfaction.ProcessEqualVariablesCondition(leftVariable, rightVariable);

                        if (satisfaction.IsSatisfied)
                        {
                            result.Add(satisfaction.SqlTable, satisfaction.ReplaceByTable);
                        }
                    }
                }
            }

            foreach (var filterCondition in filterConditionArray.OfType<EqualExpressionCondition>())
            {
                var leftOperand = filterCondition.LeftOperand;
                var rightOperand = filterCondition.RightOperand;

                if (leftOperand is ColumnExpression)
                { }
                else if (rightOperand is ColumnExpression)
                {
                    leftOperand = rightOperand;
                    rightOperand = filterCondition.LeftOperand;
                }
                else
                {
                    continue;
                }

                var leftVariable = ((ColumnExpression) leftOperand).CalculusVariable as SqlColumn;

                if (leftVariable != null)
                {
                    foreach (var satisfaction in GetSatisfactionsFromMap(satisfactionMap, leftVariable.Table))
                    {
                        if (satisfaction.IsSatisfied)
                        {
                            continue;
                        }

                        satisfaction.ProcessVariableEqualToVariablesCondition(leftVariable, rightOperand);

                        if (satisfaction.IsSatisfied)
                        {
                            result.Add(satisfaction.SqlTable, satisfaction.ReplaceByTable);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the satisfaction maps from <paramref name="satisfactionMap"/> for table <paramref name="table"/>
        /// </summary>
        private IEnumerable<SelfJoinConstraintsSatisfaction> GetSatisfactionsFromMap(Dictionary<SqlTable, Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>> satisfactionMap, SqlTable table)
        {
            foreach (var sqlTable in satisfactionMap.Keys)
            {
                if (sqlTable == table)
                {
                    foreach (var selfJoinConstraintsSatisfaction in satisfactionMap[sqlTable].Values)
                    {
                        yield return selfJoinConstraintsSatisfaction;
                    }
                }
                else
                {
                    foreach (var replaceByTable in satisfactionMap[sqlTable].Keys)
                    {
                        if (replaceByTable == table)
                        {
                            yield return satisfactionMap[sqlTable][replaceByTable];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the satisfaction from <paramref name="satisfactionMap"/> for tables <paramref name="table"/> and <paramref name="otherTable"/>
        /// </summary>
        private SelfJoinConstraintsSatisfaction GetSatisfactionFromMap(Dictionary<SqlTable, Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>> satisfactionMap, SqlTable table, SqlTable otherTable)
        {
            if (satisfactionMap.ContainsKey(table) && satisfactionMap[table].ContainsKey(otherTable))
            {
                return satisfactionMap[table][otherTable];
            }
            else if (satisfactionMap.ContainsKey(otherTable) && satisfactionMap[otherTable].ContainsKey(table))
            {
                return satisfactionMap[otherTable][table];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates initial satisfaction map
        /// </summary>
        /// <param name="presentTables">Tables present in the model</param>
        /// <param name="context">The query context</param>
        /// <returns></returns>
        private Dictionary<SqlTable, Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>> CreateInitialSatisfactionMap(List<SqlTable> presentTables, QueryContext context)
        {
            var firstTableOccurrence = new Dictionary<string, SqlTable>();
            var result = new Dictionary<SqlTable, Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>>();

            foreach (var sqlTable in presentTables)
            {
                var tableName = sqlTable.TableName;

                if (!firstTableOccurrence.ContainsKey(tableName))
                {
                    firstTableOccurrence.Add(tableName, sqlTable);
                }
                else
                {
                    var replaceByTable = firstTableOccurrence[tableName];

                    result.Add(sqlTable, new Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>());
                    result[sqlTable].Add(replaceByTable, GetSelfJoinConstraints(sqlTable, replaceByTable, context));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the self join constraints of a table
        /// </summary>
        /// <param name="sqlTable">The SQL table</param>
        /// <param name="replaceByTable">The table that will be used to replace the <paramref name="sqlTable"/></param>
        /// <param name="context">The query context</param>
        private SelfJoinConstraintsSatisfaction GetSelfJoinConstraints(SqlTable sqlTable, SqlTable replaceByTable, QueryContext context)
        {
            var tableName = sqlTable.TableName;
            var tableInfo = context.SchemaProvider.GetTableInfo(tableName);

            var constraints = tableInfo.UniqueKeys.ToList();

            if (tableInfo.PrimaryKey != null)
            {
                constraints.Add(tableInfo.PrimaryKey);
            }

            var columnConstraints = constraints
                .Select(databaseConstraint => databaseConstraint.Columns);

            return new SelfJoinConstraintsSatisfaction(sqlTable, replaceByTable, columnConstraints);
        }
    }
}