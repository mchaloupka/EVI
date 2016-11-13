using System.Collections.Generic;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils;

namespace Slp.Evi.Storage.Sparql.PostProcess.SafeAlgebra
{
    /// <summary>
    /// Class providing the ability to ascend filter pattern into LEFT JOIN
    /// </summary>
    public class AscendFilterPattern
        : BaseSparqlTransformer<object>, ISparqlPostProcess
    {
        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        public ISparqlQuery Process(ISparqlQuery query, IQueryContext context)
        {
            return TransformSparqlQuery(query, null);
        }

        /// <summary>
        /// Process the <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(FilterPattern toTransform, object data)
        {
            var newInner = TransformGraphPattern(toTransform.InnerPattern, data);

            if (newInner is FilterPattern)
            {
                var innerFilterPattern = (FilterPattern) newInner;

                return new FilterPattern(innerFilterPattern.InnerPattern, new ConjunctionExpression(new ISparqlCondition[]
                {
                    innerFilterPattern.Condition,
                    toTransform.Condition
                }));
            }
            else if (newInner != toTransform.InnerPattern)
            {
                return new FilterPattern(newInner, toTransform.Condition);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(JoinPattern toTransform, object data)
        {
            bool changed = false;
            List<IGraphPattern> newInnerPatterns = new List<IGraphPattern>();
            FilterPattern innerFilter = null;

            foreach (var joinedGraphPattern in toTransform.JoinedGraphPatterns)
            {
                var newInner = TransformGraphPattern(joinedGraphPattern, data);

                if (innerFilter == null && newInner is FilterPattern)
                {
                    changed = true;
                    innerFilter = (FilterPattern)newInner;
                }
                if (newInner != joinedGraphPattern)
                {
                    changed = true;
                    newInnerPatterns.Add(newInner);
                }
                else
                {
                    newInnerPatterns.Add(joinedGraphPattern);
                }
            }

            if (innerFilter != null)
            {
                newInnerPatterns.Add(innerFilter.InnerPattern);

                return new FilterPattern(new JoinPattern(newInnerPatterns), innerFilter.Condition);
            }
            else if (changed)
            {
                return new JoinPattern(newInnerPatterns);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(LeftJoinPattern toTransform, object data)
        {
            var transformedLeft = TransformGraphPattern(toTransform.LeftOperand, data);
            var transformedRight = TransformGraphPattern(toTransform.RightOperand, data);

            if (transformedLeft is FilterPattern || transformedRight is FilterPattern)
            {
                ISparqlCondition condition;
                IGraphPattern newRight;

                if (transformedRight is FilterPattern)
                {
                    condition =
                        new ConjunctionExpression(new ISparqlCondition[]
                        {toTransform.Condition, ((FilterPattern) transformedRight).Condition});
                    newRight = ((FilterPattern) transformedRight).InnerPattern;
                }
                else
                {
                    condition = toTransform.Condition;
                    newRight = transformedRight;
                }

                IGraphPattern result;
                if (transformedLeft is FilterPattern)
                {
                    var leftFilter = (FilterPattern) transformedLeft;

                    result = new FilterPattern(new LeftJoinPattern(leftFilter.InnerPattern, newRight, condition),
                        leftFilter.Condition);
                }
                else
                {
                    result = new LeftJoinPattern(transformedLeft, newRight, condition);
                }

                return TransformGraphPattern(result, data);
            }
            else if (transformedLeft != toTransform.LeftOperand || transformedRight != toTransform.RightOperand)
            {
                return new LeftJoinPattern(transformedLeft, transformedRight, toTransform.Condition);
            }
            else
            {
                return toTransform;
            }
        }
    }
}
