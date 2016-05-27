// Generated code, do not edit!!!

using Slp.Evi.Storage.Relational.Optimization.Optimizers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.Sources;

namespace Slp.Evi.Storage.Relational.Utils.CodeGeneration
{
    /// <summary>
    /// The base class for relational optimizers
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public class BaseRelationalOptimizerImplementation<T>
        : BaseExpressionTransformerG<BaseRelationalOptimizer<T>.OptimizationContext, IExpression, IAssignmentCondition, ISourceCondition, IFilterCondition, ICalculusSource>
    {

        /// <summary>
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ColumnExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ConcatenationExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ConstantExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="CaseExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(CaseExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="CoalesceExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(CoalesceExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="AssignmentFromExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IAssignmentCondition Transform(AssignmentFromExpressionCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISourceCondition Transform(TupleFromSourceCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="UnionedSourcesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISourceCondition Transform(UnionedSourcesCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="LeftJoinCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISourceCondition Transform(LeftJoinCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(AlwaysFalseCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(AlwaysTrueCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(ConjunctionCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(DisjunctionCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ComparisonCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(ComparisonCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(EqualExpressionCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(EqualVariablesCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(IsNullCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(NegationCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(CalculusModel toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="SqlTable"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(SqlTable toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }
    }
}
