using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.Optimization.Optimizers.IsNullOptimizerHelpers
{
    /// <summary>
    /// Calculates the <see cref="IsNullOptimizerAggregatedValues"/> from the calculus model
    /// </summary>
    public class IsNullCalculator
        : BaseExpressionTransformerG<IsNullCalculator.IsNullCalculatorParameter, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues>
    {
        /// <summary>
        /// The parameter for <see cref="IsNullCalculator"/> transformer
        /// </summary>
        public class IsNullCalculatorParameter
        {
            /// <summary>
            /// Gets the analysis result
            /// </summary>
            public IsNullOptimizerAnalyzeResult Result { get; }

            /// <summary>
            /// Gets the query context.
            /// </summary>
            public QueryContext Context { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="IsNullCalculatorParameter"/> class.
            /// </summary>
            /// <param name="result">The analysis result.</param>
            /// <param name="context">The query context.</param>
            public IsNullCalculatorParameter(IsNullOptimizerAnalyzeResult result, QueryContext context)
            {
                Result = result;
                Context = context;
            }
        }

        /// <summary>
        /// Process the <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(CalculusModel toTransform, IsNullCalculatorParameter data)
        {
            var result = new IsNullOptimizerAggregatedValues();

            foreach (var sourceCondition in toTransform.SourceConditions)
            {
                result.MergeWith(TransformSourceCondition(sourceCondition, data));
            }

            foreach (var assignmentCondition in toTransform.AssignmentConditions)
            {
                result.MergeWith(TransformAssignmentCondition(assignmentCondition, data));
            }

            foreach (var filterCondition in toTransform.FilterConditions)
            {
                result.MergeWith(TransformFilterCondition(filterCondition, data));
            }

            data.Result.GetValue(toTransform).MergeWith(result);

            return result;
        }

        /// <summary>
        /// Process the <see cref="SqlTable"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(SqlTable toTransform, IsNullCalculatorParameter data)
        {
            var aggregatedValues = new IsNullOptimizerAggregatedValues();

            var tableInfo = data.Context.SchemaProvider.GetTableInfo(toTransform.TableName);

            foreach (var calculusVariable in toTransform.Variables.OfType<SqlColumn>())
            {
                var columnInfo = tableInfo.FindColumn(calculusVariable.Name);

                if (!columnInfo.Nullable)
                {
                    aggregatedValues.AddIsNotNull(calculusVariable);
                }
            }

            return aggregatedValues;
        }

        /// <summary>
        /// Process the <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(AlwaysFalseCondition toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(AlwaysTrueCondition toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ConjunctionCondition toTransform, IsNullCalculatorParameter data)
        {
            var result = new IsNullOptimizerAggregatedValues();

            foreach (var filterCondition in toTransform.InnerConditions)
            {
                result.MergeWith(TransformFilterCondition(filterCondition, null));
            }

            return result;
        }

        /// <summary>
        /// Process the <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(DisjunctionCondition toTransform, IsNullCalculatorParameter data)
        {
            var result = new IsNullOptimizerAggregatedValues();

            foreach (var filterCondition in toTransform.InnerConditions)
            {
                result.IntersectsWith(TransformFilterCondition(filterCondition, null));
            }

            return result;
        }

        /// <summary>
        /// Process the <see cref="ComparisonCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ComparisonCondition toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(EqualExpressionCondition toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(EqualVariablesCondition toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(IsNullCondition toTransform, IsNullCalculatorParameter data)
        {
            var result = new IsNullOptimizerAggregatedValues();
            result.AddIsNullCondition(toTransform);
            return result;
        }

        /// <summary>
        /// Process the <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(NegationCondition toTransform, IsNullCalculatorParameter data)
        {
            var result = TransformFilterCondition(toTransform.InnerCondition, null);
            return result.GetInverse();
        }

        /// <summary>
        /// Process the <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(TupleFromSourceCondition toTransform, IsNullCalculatorParameter data)
        {
            var newResult = new IsNullOptimizerAnalyzeResult(toTransform.Source);
            var newAggregatedValues = TransformCalculusSource(toTransform.Source, new IsNullCalculatorParameter(newResult, data.Context));

            newResult.CopyTo(data.Result);
            return newAggregatedValues;
        }

        /// <summary>
        /// Process the <see cref="UnionedSourcesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(UnionedSourcesCondition toTransform, IsNullCalculatorParameter data)
        {
            var newAggregatedValues = new IsNullOptimizerAggregatedValues();

            foreach (var source in toTransform.Sources)
            {
                var newResult = new IsNullOptimizerAnalyzeResult(source);
                newAggregatedValues.IntersectsWith(TransformCalculusSource(source, new IsNullCalculatorParameter(newResult, data.Context)));
                newResult.CopyTo(data.Result);
            }

            return newAggregatedValues;
        }

        /// <summary>
        /// Process the <see cref="LeftJoinCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(LeftJoinCondition toTransform, IsNullCalculatorParameter data)
        {
            var rightResult = new IsNullOptimizerAnalyzeResult(toTransform.RightOperand);
            TransformCalculusSource(toTransform.RightOperand, new IsNullCalculatorParameter(rightResult, data.Context));
            rightResult.CopyTo(data.Result);

            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="AssignmentFromExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(AssignmentFromExpressionCondition toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ColumnExpression toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ConcatenationExpression toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ConstantExpression toTransform, IsNullCalculatorParameter data)
        {
            return new IsNullOptimizerAggregatedValues();
        }
    }
}