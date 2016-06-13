using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils;

namespace Slp.Evi.Storage.Sparql.PostProcess.SafeAlgebra
{
    /// <summary>
    /// Class providing the ability to ascend extend pattern over LEFT JOIN
    /// </summary>
    public class AscendExtendPattern
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
                throw new InvalidOperationException(
                    "Should never happen, the filter pattern should not be inside of left join pattern anymore");
            }

            if (newInner != toTransform.InnerPattern)
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
            ExtendPattern innerExtendPattern = null;

            foreach (var joinedGraphPattern in toTransform.JoinedGraphPatterns)
            {
                var newInner = TransformGraphPattern(joinedGraphPattern, data);

                if (data && innerExtendPattern == null && newInner is ExtendPattern)
                {
                    var innerExtend = (ExtendPattern) newInner;

                    if (IsOutOfScope(innerExtend.Expression, innerExtend.InnerPattern.AlwaysBoundVariables))
                    {
                        changed = true;
                        innerExtendPattern = (ExtendPattern)newInner;
                        continue;
                    }
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

            if (data && innerExtendPattern != null)
            {
                newInnerPatterns.Add(innerExtendPattern.InnerPattern);

                return new ExtendPattern(new JoinPattern(newInnerPatterns), innerExtendPattern.VariableName, innerExtendPattern.Expression);
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
        protected override IGraphPattern Transform(LeftJoinPattern toTransform, bool data)
        {
            var transformedLeft = TransformGraphPattern(toTransform.LeftOperand, data);
            var transformedRight = TransformGraphPattern(toTransform.RightOperand, true);

            if (data && transformedLeft is ExtendPattern)
            {
                var leftExtend = (ExtendPattern) transformedLeft;
                var result =
                    new ExtendPattern(
                        new LeftJoinPattern(leftExtend.InnerPattern, transformedRight,
                            ReplaceVariableInExpression(toTransform.Condition, leftExtend.VariableName,
                                leftExtend.Expression)), leftExtend.VariableName, leftExtend.Expression);

                return TransformGraphPattern(result, data);
            }
            else if (transformedRight is ExtendPattern)
            {
                var rightExtend = (ExtendPattern) transformedRight;

                var providedVariables =
                    new HashSet<string>(toTransform.AlwaysBoundVariables);
                providedVariables.IntersectWith(rightExtend.Expression.NeededVariables);

                if (providedVariables.Count < rightExtend.Expression.NeededVariables.Count())
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Process the <see cref="ExtendPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(ExtendPattern toTransform, bool data)
        {
            var newInner = TransformGraphPattern(toTransform.InnerPattern, data);

            if (data)
            {
                if (!IsOutOfScope(toTransform.Expression, toTransform.InnerPattern.AlwaysBoundVariables))
                {
                    if (newInner is ExtendPattern)
                    {
                        var innerExtend = (ExtendPattern) newInner;

                        if (IsOutOfScope(innerExtend.Expression, innerExtend.InnerPattern.AlwaysBoundVariables))
                        {
                            return
                                new ExtendPattern(
                                    new ExtendPattern(innerExtend.InnerPattern, toTransform.VariableName,
                                        toTransform.Expression), innerExtend.VariableName, innerExtend.Expression);
                        }
                    }
                    else if (newInner is LeftJoinPattern)
                    {
                        var innerLeftJoin = (LeftJoinPattern) newInner;

                        if (!IsOutOfScope(toTransform.Expression, innerLeftJoin.LeftOperand.AlwaysBoundVariables))
                        {
                            return
                                new LeftJoinPattern(
                                    new ExtendPattern(innerLeftJoin.LeftOperand, toTransform.VariableName,
                                        toTransform.Expression), innerLeftJoin.RightOperand, innerLeftJoin.Condition);
                        }
                    }
                }
            }

            if (newInner != toTransform.InnerPattern)
            {
                return new ExtendPattern(newInner, toTransform.VariableName, toTransform.Expression);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(MinusPattern toTransform, bool data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Process the <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(UnionPattern toTransform, bool data)
        {
            return base.Transform(toTransform, false);
        }

        private ISparqlCondition ReplaceVariableInExpression(ISparqlCondition condition, string variableName, ISparqlExpression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether <paramref name="expression"/> may be out of scope regarding to the always bound variables <paramref name="alwaysBoundVariables"/>.
        /// </summary>
        private bool IsOutOfScope(ISparqlExpression expression, IEnumerable<string> alwaysBoundVariables)
        {
            var providedVariables =
                    new HashSet<string>(alwaysBoundVariables);
            providedVariables.IntersectWith(expression.NeededVariables);

            return providedVariables.Count < expression.NeededVariables.Count();
        }
    }
}
