// Generated code, do not edit!!!

using System;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Optimization.Optimizers;

namespace Slp.Evi.Storage.Sparql.Utils.CodeGeneration
{
    /// <summary>
    /// The base class for SPARQL optimizers
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public class BaseSparqlOptimizerImplementation<T>
        : BaseSparqlExpressionTransformerG<BaseSparqlOptimizer<T>.OptimizationContext, ISparqlExpression, IGraphPattern, ISparqlQuery>
    {
        /// <summary>
        /// Process the <see cref="ISparqlQuery"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        public virtual ISparqlQuery TransformSparqlQuery(ISparqlQuery toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            if (toTransform is IModifier)
            {
                return TransformModifier((IModifier)toTransform, data);
            }
            else if (toTransform is IGraphPattern)
            {
                return TransformGraphPattern((IGraphPattern)toTransform, data);
            }
            else
            {
                throw new ArgumentException("Unexpected type of parameter", nameof(toTransform));
            }
        }

        /// <summary>
        /// Process the <see cref="IsBoundExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(IsBoundExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="BooleanTrueExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(BooleanTrueExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="BooleanFalseExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(BooleanFalseExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="NegationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(NegationExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="VariableExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(VariableExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConjunctionExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(ConjunctionExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ComparisonExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(ComparisonExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="NodeExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(NodeExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="DisjunctionExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(DisjunctionExpression toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="EmptyPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(EmptyPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(FilterPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="NotMatchingPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(NotMatchingPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(GraphPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(JoinPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(LeftJoinPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(MinusPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(TriplePattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(UnionPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="RestrictedTriplePattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(RestrictedTriplePattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ExtendPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(ExtendPattern toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="SelectModifier"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlQuery Transform(SelectModifier toTransform, BaseSparqlOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }
    }
}
