using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers
{
    /// <summary>
    /// This class provides the ability to extract <see cref="CaseExpression"/> in
    /// <see cref="ComparisonCondition"/>.
    /// </summary>
    public class CaseExpressionToConditionOptimizer
        : BaseRelationalOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseExpressionToConditionOptimizer"/> class.
        /// </summary>
        public CaseExpressionToConditionOptimizer() 
            : base(new CaseExpressionToConditionOptimizerImplementation())
        { }

        /// <summary>
        /// Implementation for <see cref="CaseExpressionToConditionOptimizer"/>
        /// </summary>
        public class CaseExpressionToConditionOptimizerImplementation
            : BaseRelationalOptimizerImplementation<object>
        {
            /// <summary>
            /// Process the <see cref="ComparisonCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(ComparisonCondition toTransform, OptimizationContext data)
            {
                var leftStatements = ExtractStatements(toTransform.LeftOperand).ToList();
                var rightStatements = ExtractStatements(toTransform.RightOperand).ToList();

                if (leftStatements.Count > 1 || rightStatements.Count > 1)
                {
                    var conditions = new List<IFilterCondition>();

                    foreach (var leftStatement in leftStatements)
                    {
                        foreach (var rightStatement in rightStatements)
                        {
                            var compareCondition = new ComparisonCondition(leftStatement.Item2, rightStatement.Item2,
                                toTransform.ComparisonType);

                            var conjunctions = new List<IFilterCondition>();
                            conjunctions.AddRange(ExtractConditions(leftStatement.Item1));
                            conjunctions.AddRange(ExtractConditions(rightStatement.Item1));

                            conjunctions.Add(compareCondition);

                            conditions.Add(new ConjunctionCondition(conjunctions));
                        }
                    }

                    return new DisjunctionCondition(conditions);
                }
                else
                {
                    return toTransform;
                }
            }

            /// <summary>
            /// Extracts the statements from expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            private IEnumerable<Tuple<IFilterCondition, IExpression>> ExtractStatements(IExpression expression)
            {
                if (expression is CaseExpression caseExpression)
                {
                    foreach (var statement in caseExpression.Statements)
                    {
                        foreach (var extractStatement in ExtractStatements(statement.Expression))
                        {
                            var conditions = new List<IFilterCondition>();
                            conditions.AddRange(ExtractConditions(statement.Condition));
                            conditions.AddRange(ExtractConditions(extractStatement.Item1));
                            var condition = new ConjunctionCondition(conditions);

                            yield return new Tuple<IFilterCondition, IExpression>(condition, extractStatement.Item2);
                        }
                    }
                }
                else
                {
                    yield return new Tuple<IFilterCondition, IExpression>(new AlwaysTrueCondition(), expression);
                }
            }

            /// <summary>
            /// Extracts the conditions for conjunction.
            /// </summary>
            private IEnumerable<IFilterCondition> ExtractConditions(IFilterCondition condition)
            {
                if (condition is AlwaysTrueCondition)
                {
                    return new IFilterCondition[0];
                }
                else if (condition is ConjunctionCondition conjunctionCondition)
                {
                    return conjunctionCondition.InnerConditions.SelectMany(ExtractConditions);
                }
                else
                {
                    return new IFilterCondition[] {condition};
                }
            }
        }
    }
}
