using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils;

namespace Slp.Evi.Storage.Sparql.PostProcess.SafeAlgebra
{
    /// <summary>
    /// Class providing the ability to remove nested optionals
    /// </summary>
    public class RemoveNestedOptionals
        : BaseSparqlTransformer<bool>, ISparqlPostProcess
    {
        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        public ISparqlQuery Process(ISparqlQuery query, QueryContext context)
        {
            return TransformSparqlQuery(query, false);
        }

        /// <summary>
        /// Process the <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(FilterPattern toTransform, bool data)
        {
            var newInner = TransformGraphPattern(toTransform.InnerPattern, data);

            if (data)
            {
                throw new InvalidOperationException("Should never happen, the filter pattern should not be inside of left join pattern anymore");
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
        /// Process the <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(GraphPattern toTransform, bool data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Process the <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(JoinPattern toTransform, bool data)
        {
            bool changed = false;
            List<IGraphPattern> newInnerPatterns = new List<IGraphPattern>();
            LeftJoinPattern innerLeftJoin = null;

            foreach (var joinedGraphPattern in toTransform.JoinedGraphPatterns)
            {
                var newInner = TransformGraphPattern(joinedGraphPattern, data);

                if (data && innerLeftJoin == null && newInner is LeftJoinPattern)
                {
                    changed = true;
                    innerLeftJoin = (LeftJoinPattern)newInner;
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

            if (data && innerLeftJoin != null)
            {
                newInnerPatterns.Add(innerLeftJoin.LeftOperand);

                return new LeftJoinPattern(
                    new JoinPattern(newInnerPatterns),
                    innerLeftJoin.RightOperand, innerLeftJoin.Condition);
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

        protected override IGraphPattern Transform(LeftJoinPattern toTransform, bool data)
        {
            if (data)
            {
                return toTransform;
            }
            else
            {
                var newLeft = TransformGraphPattern(toTransform.LeftOperand, false);
                var newRight = TransformGraphPattern(toTransform.RightOperand, true);

                if (newRight is LeftJoinPattern)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected override IGraphPattern Transform(ExtendPattern toTransform, bool data)
        {
            throw new NotImplementedException();
        }

        protected override IGraphPattern Transform(MinusPattern toTransform, bool data)
        {
            throw new NotImplementedException();
        }

        protected override IGraphPattern Transform(UnionPattern toTransform, bool data)
        {
            throw new NotImplementedException();
        }
    }
}
