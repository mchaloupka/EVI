using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query.Sources;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// The type representing satisfaction map
    /// </summary>
    public class SatisfactionMap
    {
        /// <summary>
        /// The satisfaction storage
        /// </summary>
        private readonly Dictionary<SqlTable, Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>> _storeDictionary;

        /// <summary>
        /// The present tables
        /// </summary>
        private readonly List<SqlTable> _presentTables;

        /// <summary>
        /// The context
        /// </summary>
        private readonly IQueryContext _context;

        /// <summary>
        /// The satisfied satisfactions
        /// </summary>
        private readonly List<SelfJoinConstraintsSatisfaction> _satisfiedSatisfactions; 

        /// <summary>
        /// Prevents a default instance of the <see cref="SatisfactionMap"/> class from being created.
        /// </summary>
        private SatisfactionMap(List<SqlTable> presentTables, IQueryContext context)
        {
            _presentTables = presentTables;
            _context = context;
            _satisfiedSatisfactions = new List<SelfJoinConstraintsSatisfaction>();
            _storeDictionary = new Dictionary<SqlTable, Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>>();
        }

        /// <summary>
        /// Creates the initial satisfaction map.
        /// </summary>
        public SatisfactionMap CreateInitialSatisfactionMap()
        {
            return CreateInitialSatisfactionMap(_presentTables, _context);
        }

        /// <summary>
        /// Creates the initial satisfaction map
        /// </summary>
        /// <param name="presentTables">Tables present in the model</param>
        /// <param name="context">The query context</param>
        /// <returns></returns>
        public static SatisfactionMap CreateInitialSatisfactionMap(List<SqlTable> presentTables, IQueryContext context)
        {
            var firstTableOccurrence = new Dictionary<string, SqlTable>();
            var result = new SatisfactionMap(presentTables, context);

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

                    result._storeDictionary.Add(sqlTable, new Dictionary<SqlTable, SelfJoinConstraintsSatisfaction>());
                    result._storeDictionary[sqlTable].Add(replaceByTable, GetSelfJoinConstraints(sqlTable, replaceByTable, context));
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
        private static SelfJoinConstraintsSatisfaction GetSelfJoinConstraints(SqlTable sqlTable, SqlTable replaceByTable, IQueryContext context)
        {
            var tableName = sqlTable.TableName;
            var tableInfo = context.SchemaProvider.GetTableInfo(tableName);

            var constraints = tableInfo.UniqueKeys.ToList();

            if (tableInfo.PrimaryKey != null)
            {
                constraints.Add(tableInfo.PrimaryKey);
            }

            return new SelfJoinConstraintsSatisfaction(sqlTable, replaceByTable,
                constraints.Select(databaseConstraint => new UniqueConstraint(databaseConstraint)));
        }

        /// <summary>
        /// Gets the satisfactions for table <paramref name="table"/>
        /// </summary>
        public IEnumerable<SelfJoinConstraintsSatisfaction> GetSatisfactionsFromMap(SqlTable table)
        {
            foreach (var sqlTable in _storeDictionary.Keys)
            {
                if (sqlTable == table)
                {
                    foreach (var selfJoinConstraintsSatisfaction in _storeDictionary[sqlTable].Values)
                    {
                        yield return selfJoinConstraintsSatisfaction;
                    }
                }
                else
                {
                    foreach (var replaceByTable in _storeDictionary[sqlTable].Keys)
                    {
                        if (replaceByTable == table)
                        {
                            yield return _storeDictionary[sqlTable][replaceByTable];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the satisfaction for tables <paramref name="table"/> and <paramref name="otherTable"/>
        /// </summary>
        public SelfJoinConstraintsSatisfaction GetSatisfactionFromMap(SqlTable table, SqlTable otherTable)
        {
            if (_storeDictionary.ContainsKey(table) && _storeDictionary[table].ContainsKey(otherTable))
            {
                return _storeDictionary[table][otherTable];
            }
            else if (_storeDictionary.ContainsKey(otherTable) && _storeDictionary[otherTable].ContainsKey(table))
            {
                return _storeDictionary[otherTable][table];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Marks <paramref name="satisfaction"/>as satisfied.
        /// </summary>
        public void MarkAsSatisfied(SelfJoinConstraintsSatisfaction satisfaction)
        {
            _satisfiedSatisfactions.Add(satisfaction);
        }

        /// <summary>
        /// Gets the satisfied satisfactions.
        /// </summary>
        public IEnumerable<SelfJoinConstraintsSatisfaction> GetSatisfiedSatisfactions()
        {
            return _satisfiedSatisfactions;
        }

        /// <summary>
        /// Intersects with the <paramref name="other"/> map.
        /// </summary>
        public void IntersectWith(SatisfactionMap other)
        {
            if (other._context != _context || other._presentTables != _presentTables)
            {
                throw new ArgumentException("Cannot intersect with differently originated satisfaction map",
                    nameof(other));
            }

            foreach (var sqlTable in _storeDictionary.Keys)
            {
                foreach (var replaceByTable in _storeDictionary[sqlTable].Keys)
                {
                    var satisfaction = _storeDictionary[sqlTable][replaceByTable];
                    var wasSatisfied = satisfaction.IsSatisfied;

                    satisfaction.IntersectWith(other._storeDictionary[sqlTable][replaceByTable]);

                    if (wasSatisfied && !satisfaction.IsSatisfied)
                    {
                        _satisfiedSatisfactions.Remove(satisfaction);
                    }
                }
            }
        }

        /// <summary>
        /// Merges with the <paramref name="other"/> map.
        /// </summary>
        public void MergeWith(SatisfactionMap other)
        {
            if (other._context != _context || other._presentTables != _presentTables)
            {
                throw new ArgumentException("Cannot merge with differently originated satisfaction map",
                    nameof(other));
            }

            foreach (var sqlTable in _storeDictionary.Keys)
            {
                foreach (var replaceByTable in _storeDictionary[sqlTable].Keys)
                {
                    var satisfaction = _storeDictionary[sqlTable][replaceByTable];
                    var wasSatisfied = satisfaction.IsSatisfied;

                    satisfaction.MergeWith(other._storeDictionary[sqlTable][replaceByTable]);

                    if (!wasSatisfied && satisfaction.IsSatisfied)
                    {
                        _satisfiedSatisfactions.Add(satisfaction);
                    }
                }
            }
        }
    }
}