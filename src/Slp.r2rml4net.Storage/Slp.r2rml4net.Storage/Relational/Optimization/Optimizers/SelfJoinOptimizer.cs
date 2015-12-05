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
        private readonly SelfJoinConstraintsCalculator _selfJoinConstraintsCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfJoinOptimizer"/> class.
        /// </summary>
        public SelfJoinOptimizer() 
            : base(new SelfJoinOptimizer.SelfJoinOptimizerImplementation())
        {
            _selfJoinConstraintsCalculator = new SelfJoinConstraintsCalculator();
        }

        /// <summary>
        /// Processes the visit of <see cref="CalculusModel" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected override ICalculusSource ProcessVisit(CalculusModel toVisit, OptimizationContext data)
        {
            var newContext = new OptimizationContext()
            {
                Data = new SelfJoinOptimizerData(),
                Context = data.Context
            };

            var result = base.ProcessVisit(toVisit, newContext);

            data.Data.LoadOtherData(newContext.Data);

            return result;
        }

        /// <summary>
        /// Process the <see cref="CalculusModel" />
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(CalculusModel toTransform, OptimizationContext data)
        {
            var presentTables = toTransform.SourceConditions
                .OfType<TupleFromSourceCondition>()
                .Select(x => x.Source)
                .OfType<SqlTable>()
                .ToList();

            var processedSelfJoinConditions = _selfJoinConstraintsCalculator.ProcessSelfJoinConditions(toTransform.FilterConditions,
                presentTables, data);

            if (processedSelfJoinConditions.Keys.Any())
            {
                var conditions = new List<ICondition>();

                foreach (var sourceCondition in toTransform.SourceConditions)
                {
                    if (!(sourceCondition is TupleFromSourceCondition))
                    {
                        conditions.Add(sourceCondition);
                        continue;
                    }

                    var tupleFromSourceCondition = (TupleFromSourceCondition) sourceCondition;

                    if (!(tupleFromSourceCondition.Source is SqlTable))
                    {
                        conditions.Add(tupleFromSourceCondition);
                        continue;
                    }

                    var sqlTable = (SqlTable) tupleFromSourceCondition.Source;

                    if (!processedSelfJoinConditions.ContainsKey(sqlTable))
                    {
                        conditions.Add(tupleFromSourceCondition);
                        continue;
                    }

                    var targetTable = processedSelfJoinConditions[sqlTable];

                    foreach (var sqlColumn in tupleFromSourceCondition.CalculusVariables.Cast<SqlColumn>())
                    {
                        var targetColumn = targetTable.GetVariable(sqlColumn.Name);

                        data.Data.AddReplaceColumnInformation(sqlColumn, targetColumn);
                    }
                }

                conditions.AddRange(toTransform.FilterConditions);
                conditions.AddRange(toTransform.AssignmentConditions);

                var transformed = new CalculusModel(toTransform.Variables, conditions);
                return base.Transform(transformed, data);
            }
            else
            {
                return base.Transform(toTransform, data);
            }
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
