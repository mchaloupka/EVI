using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.SelfJoinOptimizerHelpers;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers
{
    /// <summary>
    /// Self-join optimizer
    /// </summary>
    public class SelfJoinOptimizer
        : BaseRelationalOptimizer<SelfJoinOptimizerData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelfJoinOptimizer"/> class.
        /// </summary>
        public SelfJoinOptimizer() 
            : base(new SelfJoinOptimizerImplementation())
        { }

        /// <summary>
        /// Processes the visit of <see cref="CalculusModel" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected override ICalculusSource ProcessVisit(CalculusModel toVisit, OptimizationContext data)
        {
            // TODO: Propagate the replacement information upwards

            return base.ProcessVisit(toVisit, new OptimizationContext()
            {
                Data = new SelfJoinOptimizerData(),
                Context = data.Context
            });
        }

        /// <summary>
        /// Processes the visit of <see cref="SqlTable" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected override ICalculusSource ProcessVisit(SqlTable toVisit, OptimizationContext data)
        {
            return toVisit;
        }

        /// <summary>
        /// Process the <see cref="CalculusModel" />
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(CalculusModel toTransform, OptimizationContext data)
        {
            //var presentTables = new HashSet<SqlTable>(toTransform.SourceConditions.OfType<TupleFromSourceCondition>()
            //        .Select(sourceCondition => sourceCondition.Source as SqlTable));

            //var findSelfJoins = ProcessSelfJoinConditions(toTransform.FilterConditions, presentTables, data);

            //if (findSelfJoins.Count > 0)
            //{
            //    var newConditions = new List<ICondition>();

            //    foreach (var sourceCondition in toTransform.SourceConditions)
            //    {
            //        var sqlTable = (sourceCondition as TupleFromSourceCondition)?.Source as SqlTable;

            //        if (sqlTable != null)
            //        {
            //            if (findSelfJoins.ContainsKey(sqlTable.TableName) && findSelfJoins[sqlTable.TableName][0] != sqlTable)
            //            {
            //                data.Data.AddReplaceTableInformation(sqlTable, findSelfJoins[sqlTable.TableName][0]);
            //                continue;
            //            }
            //        }

            //        newConditions.Add(sourceCondition);
            //    }

            //    newConditions.AddRange(toTransform.AssignmentConditions);
            //    newConditions.AddRange(toTransform.FilterConditions);

            //    return new CalculusModel(toTransform.Variables, newConditions);
            //}
            //else
            //{
                return base.Transform(toTransform, data);
            //}
        }

        /// <summary>
        /// Processes the self join conditions.
        /// </summary>
        /// <param name="filterConditions">The filter conditions.</param>
        /// <param name="presentTables"></param>
        /// <param name="data">The data.</param>
        /// <returns>List of all tables that are self joined</returns>
        private Dictionary<string, List<SqlTable>> ProcessSelfJoinConditions(IEnumerable<IFilterCondition> filterConditions, HashSet<SqlTable> presentTables, OptimizationContext data)
        {
            var result = new Dictionary<string, List<SqlTable>>();

            foreach (var filterCondition in filterConditions.OfType<EqualVariablesCondition>())
            {
                var leftVariable = filterCondition.LeftVariable as SqlColumn;
                var rightVariable = filterCondition.RightVariable as SqlColumn;

                if (leftVariable != null && rightVariable != null
                    && presentTables.Contains(leftVariable.Table) && presentTables.Contains(rightVariable.Table))
                {
                    
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the initial data.
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="context">The context</param>
        protected override SelfJoinOptimizerData CreateInitialData(RelationalQuery query, QueryContext context)
        {
            return new SelfJoinOptimizerData();
        }

        /// <summary>
        /// The implementation of <see cref="SelfJoinOptimizer"/>
        /// </summary>
        private class SelfJoinOptimizerImplementation
            : BaseRelationalOptimizerImplementation<SelfJoinOptimizerData>
        {
            
        }
    }
}
