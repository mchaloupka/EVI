using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            LeftJoinPattern innerLeftJoinPattern = null;

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
                else if (data && innerLeftJoinPattern == null && newInner is LeftJoinPattern)
                {
                    innerLeftJoinPattern = (LeftJoinPattern) newInner;
                    changed = true;
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

                if (innerLeftJoinPattern != null)
                {
                    newInnerPatterns.Add(innerLeftJoinPattern);
                }

                return new ExtendPattern(new JoinPattern(newInnerPatterns), innerExtendPattern.VariableName,
                    innerExtendPattern.Expression);
            }
            else if (data && innerLeftJoinPattern != null)
            {
                newInnerPatterns.Add(innerLeftJoinPattern.LeftOperand);

                return new LeftJoinPattern(new JoinPattern(newInnerPatterns), innerLeftJoinPattern.RightOperand,
                    innerLeftJoinPattern.Condition);
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

            if (data)
            {
                if (transformedLeft is ExtendPattern)
                {
                    var leftExtend = (ExtendPattern) transformedLeft;

                    if (IsOutOfScope(leftExtend.Expression, leftExtend.InnerPattern.AlwaysBoundVariables))
                    {
                        return
                            new ExtendPattern(
                                new LeftJoinPattern(leftExtend.InnerPattern, transformedRight,
                                    ReplaceVariableInCondition(toTransform.Condition, leftExtend.VariableName,
                                        leftExtend.Expression)), leftExtend.VariableName, leftExtend.Expression);
                    }
                }
            }

            if (transformedRight is ExtendPattern)
            {
                var rightExtend = (ExtendPattern) transformedRight;

                if (IsOutOfScope(rightExtend.Expression, rightExtend.InnerPattern.AlwaysBoundVariables))
                {
                    // TODO: Handle the scenario when the bound variable causes that the optional should not apply
                    // It should be handled by additional join condition detecting that the variable is bound to correct value or not bound from the extend at all

                    return 
                        new ExtendPattern(
                            new LeftJoinPattern(transformedLeft, rightExtend.InnerPattern,
                                ReplaceVariableInCondition(toTransform.Condition, rightExtend.VariableName, 
                                rightExtend.Expression)), rightExtend.VariableName, rightExtend.Expression);
                }
            }

            if (transformedLeft != toTransform.LeftOperand || transformedRight != toTransform.RightOperand)
            {
                return new LeftJoinPattern(transformedLeft, transformedRight, toTransform.Condition);
            }
            else
            {
                return toTransform;
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

        /// <summary>
        /// Replaces the variable in condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="expression">The expression which will replace the variable with <paramref name="variableName"/>.</param>
        private ISparqlCondition ReplaceVariableInCondition(ISparqlCondition condition, string variableName, ISparqlExpression expression)
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
