using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils;
using Slp.Evi.Storage.Sparql.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Sparql.SafeAlgebra
{
    /// <summary>
    /// Class providing the ability to remove nested optionals
    /// </summary>
    public class RemoveNestedOptionals
        : BaseSparqlTransformer<bool>
    {
        /// <summary>
        /// Process the <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(FilterPattern toTransform, bool data)
        {
            var newInner = TransformGraphPattern(toTransform, data);

            if (data && (newInner is LeftJoinPattern))
            {
                var innerLeftJoin = (LeftJoinPattern) newInner;

                return new LeftJoinPattern(
                        new FilterPattern(innerLeftJoin.LeftOperand, toTransform.Condition),
                        innerLeftJoin.RightOperand);
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
                    innerLeftJoin.RightOperand);
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
