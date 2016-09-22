using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.PostProcess.Optimizers.SelfJoinOptimizerHelpers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers
{
    /// <summary>
    /// Self-join optimizer
    /// </summary>
    public class SelfJoinOptimizer
        : BaseRelationalOptimizer<SelfJoinOptimizerData>
    {
        private readonly SelfJoinConstraintsCalculator _selfJoinConstraintsCalculator;
        private readonly SelfJoinValueBindersOptimizerImplementation _selfJoinValueBinderOptimizerImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfJoinOptimizer"/> class.
        /// </summary>
        public SelfJoinOptimizer() 
            : base(new SelfJoinOptimizerImplementation())
        {
            _selfJoinConstraintsCalculator = new SelfJoinConstraintsCalculator();
            _selfJoinValueBinderOptimizerImplementation = new SelfJoinValueBindersOptimizerImplementation(OptimizerImplementation);
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
        /// Optimizes the relational query.
        /// </summary>
        /// <param name="relationalQuery">The relational query.</param>
        /// <param name="optimizationContext">The optimization context.</param>
        protected override RelationalQuery OptimizeRelationalQuery(RelationalQuery relationalQuery, OptimizationContext optimizationContext)
        {
            var valueBinders = new List<IValueBinder>();
            var changed = false;

            foreach (var valueBinder in relationalQuery.ValueBinders)
            {
                var transformed = (IValueBinder)valueBinder.Accept(_selfJoinValueBinderOptimizerImplementation, optimizationContext);

                if(transformed != valueBinder)
                {
                    changed = true;
                }

                valueBinders.Add(transformed);
            }

            if (changed)
            {
                return new RelationalQuery(relationalQuery.Model, valueBinders);
            }
            else
            {
                return relationalQuery;
            }
        }

        /// <summary>
        /// The implementation of <see cref="SelfJoinOptimizer"/>
        /// </summary>
        private class SelfJoinOptimizerImplementation
            : BaseRelationalOptimizerImplementation<SelfJoinOptimizerData>
        {
            /// <summary>
            /// Process the <see cref="ColumnExpression"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IExpression Transform(ColumnExpression toTransform, OptimizationContext data)
            {
                if (data.Data.IsReplaced(toTransform.CalculusVariable))
                {
                    return new ColumnExpression(data.Context, data.Data.GetReplacingVariable(toTransform.CalculusVariable), toTransform.IsUri);
                }
                else
                {
                    return toTransform;
                }
            }

            /// <summary>
            /// Process the <see cref="EqualVariablesCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(EqualVariablesCondition toTransform, OptimizationContext data)
            {
                if (data.Data.IsReplaced(toTransform.LeftVariable)
                    || data.Data.IsReplaced(toTransform.RightVariable))
                {
                    var leftVariable = toTransform.LeftVariable;
                    var rightVariable = toTransform.RightVariable;

                    if (data.Data.IsReplaced(leftVariable))
                    {
                        leftVariable = data.Data.GetReplacingVariable(leftVariable);
                    }

                    if (data.Data.IsReplaced(rightVariable))
                    {
                        rightVariable = data.Data.GetReplacingVariable(rightVariable);
                    }

                    if (leftVariable == rightVariable)
                    {
                        return new AlwaysTrueCondition();
                    }
                    else
                    {
                        return new EqualVariablesCondition(leftVariable, rightVariable);
                    }
                }
                else
                {
                    return toTransform;
                }
            }

            /// <summary>
            /// Process the <see cref="IsNullCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(IsNullCondition toTransform, OptimizationContext data)
            {
                if (data.Data.IsReplaced(toTransform.Variable))
                {
                    return new IsNullCondition(data.Data.GetReplacingVariable(toTransform.Variable));
                }
                else
                {
                    return base.Transform(toTransform, data);
                }
            }

            /// <summary>
            /// Process the <see cref="CalculusModel"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override ICalculusSource Transform(CalculusModel toTransform, OptimizationContext data)
            {
                if (toTransform.Variables.Any(x => data.Data.IsReplaced(x)))
                {
                    var variables = toTransform.Variables
                        .Select(x => data.Data.IsReplaced(x) ? data.Data.GetReplacingVariable(x) : x)
                        .Distinct()
                        .ToList();

                    var conditions = new List<ICondition>();
                    conditions.AddRange(toTransform.FilterConditions);
                    conditions.AddRange(toTransform.AssignmentConditions);
                    conditions.AddRange(toTransform.SourceConditions);

                    return new CalculusModel(variables, conditions);
                }
                else
                {
                    return toTransform;
                }
            }

            /// <summary>
            /// Process the <see cref="TupleFromSourceCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override ISourceCondition Transform(TupleFromSourceCondition toTransform, OptimizationContext data)
            {
                if (toTransform.CalculusVariables.Any(x => data.Data.IsReplaced(x)))
                {
                    var variables = toTransform.CalculusVariables
                        .Select(x => data.Data.IsReplaced(x) ? data.Data.GetReplacingVariable(x) : x)
                        .Distinct()
                        .ToList();

                    return new TupleFromSourceCondition(variables, toTransform.Source);
                }
                else
                {
                    return base.Transform(toTransform, data);
                }
            }

            /// <summary>
            /// Process the <see cref="UnionedSourcesCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override ISourceCondition Transform(UnionedSourcesCondition toTransform, OptimizationContext data)
            {
                if (toTransform.CalculusVariables.Any(x => data.Data.IsReplaced(x))
                    || data.Data.IsReplaced(toTransform.CaseVariable))
                {
                    var variables = toTransform.CalculusVariables
                        .Select(x => data.Data.IsReplaced(x) ? data.Data.GetReplacingVariable(x) : x)
                        .Distinct()
                        .ToList();

                    var caseVariable = data.Data.IsReplaced(toTransform.CaseVariable)
                        ? data.Data.GetReplacingVariable(toTransform.CaseVariable)
                        : toTransform.CaseVariable;

                    return new UnionedSourcesCondition(caseVariable, variables, toTransform.Sources);
                }

                return base.Transform(toTransform, data);
            }

            /// <summary>
            /// Process the <see cref="ModifiedCalculusModel"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override ICalculusSource Transform(ModifiedCalculusModel toTransform, OptimizationContext data)
            {
                var changed = false;

                var newOrderingParts = new List<ModifiedCalculusModel.OrderingPart>();
                foreach (var orderingPart in toTransform.Ordering)
                {
                    var transformedExpression = TransformExpression(orderingPart.Expression, data);
                    if (transformedExpression != orderingPart.Expression)
                    {
                        var newOrderingPart = new ModifiedCalculusModel.OrderingPart(transformedExpression,
                            orderingPart.IsDescending);

                        newOrderingParts.Add(newOrderingPart);
                        changed = true;
                    }
                    else
                    {
                        newOrderingParts.Add(orderingPart);
                    }
                }

                if (changed)
                {
                    return new ModifiedCalculusModel(toTransform.InnerModel, newOrderingParts, toTransform.Limit,
                        toTransform.Offset);
                }
                else
                {
                    return toTransform;
                }
            }
        }
    }
}
