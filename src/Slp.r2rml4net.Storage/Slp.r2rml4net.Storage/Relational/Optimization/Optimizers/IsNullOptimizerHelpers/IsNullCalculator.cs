using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Assignment;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.IsNullOptimizerHelpers
{
    /// <summary>
    /// Calculates the <see cref="IsNullOptimizerAggregatedValues"/> from the calculus model
    /// </summary>
    public class IsNullCalculator
        : BaseExpressionTransformerG<IsNullOptimizerAnalyzeResult, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues, IsNullOptimizerAggregatedValues>
    {
        /// <summary>
        /// Process the <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(CalculusModel toTransform, IsNullOptimizerAnalyzeResult data)
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

            return result;
        }

        /// <summary>
        /// Process the <see cref="SqlTable"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(SqlTable toTransform, IsNullOptimizerAnalyzeResult data)
        {
            // TODO: Here should be detection from SQL properties

            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(AlwaysFalseCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(AlwaysTrueCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ConjunctionCondition toTransform, IsNullOptimizerAnalyzeResult data)
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
        protected override IsNullOptimizerAggregatedValues Transform(DisjunctionCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            var result = new IsNullOptimizerAggregatedValues();

            foreach (var filterCondition in toTransform.InnerConditions)
            {
                result.IntersectsWith(TransformFilterCondition(filterCondition, null));
            }

            return result;
        }

        /// <summary>
        /// Process the <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(EqualExpressionCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(EqualVariablesCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(IsNullCondition toTransform, IsNullOptimizerAnalyzeResult data)
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
        protected override IsNullOptimizerAggregatedValues Transform(NegationCondition toTransform, IsNullOptimizerAnalyzeResult data)
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
        protected override IsNullOptimizerAggregatedValues Transform(TupleFromSourceCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            var newResult = new IsNullOptimizerAnalyzeResult(toTransform.Source);
            var newAggregatedValues = TransformCalculusSource(toTransform.Source, newResult);

            newResult.CopyTo(data);
            return newAggregatedValues;
        }

        /// <summary>
        /// Process the <see cref="UnionedSourcesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(UnionedSourcesCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            var newAggregatedValues = new IsNullOptimizerAggregatedValues();

            foreach (var source in toTransform.Sources)
            {
                var newResult = new IsNullOptimizerAnalyzeResult(source);
                newAggregatedValues.IntersectsWith(TransformCalculusSource(source, newResult));
                newResult.CopyTo(data);
            }

            return newAggregatedValues;
        }

        /// <summary>
        /// Process the <see cref="AssignmentFromExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(AssignmentFromExpressionCondition toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ColumnExpression toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ConcatenationExpression toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IsNullOptimizerAggregatedValues Transform(ConstantExpression toTransform, IsNullOptimizerAnalyzeResult data)
        {
            return new IsNullOptimizerAggregatedValues();
        }
    }
}