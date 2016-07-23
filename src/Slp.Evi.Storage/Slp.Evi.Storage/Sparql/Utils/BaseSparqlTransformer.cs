using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Sparql.Utils
{
    /// <summary>
    /// Base class for SPARQL transformations
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public class BaseSparqlTransformer<T>
        : BaseSparqlExpressionTransformerG<T, ISparqlExpression, IGraphPattern, ISparqlQuery>
    {
        /// <summary>
        /// Transforms the <see cref="ISparqlQuery" />.
        /// </summary>
        /// <param name="instance">The instance to transform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public ISparqlQuery TransformSparqlQuery(ISparqlQuery instance, T data)
        {
            if (instance is IModifier)
            {
                return TransformModifier((IModifier)instance, data);
            }
            else if (instance is IGraphPattern)
            {
                return TransformGraphPattern((IGraphPattern)instance, data);
            }
            else
            {
                throw new ArgumentException("Unexpected type of parameter", nameof(instance));
            }
        }

        /// <summary>
        /// Transforms the <see cref="ISparqlCondition" />.
        /// </summary>
        /// <param name="condition">The condition to transform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        private ISparqlCondition TransformSparqlCondition(ISparqlCondition condition, T data)
        {
            return (ISparqlCondition)TransformSparqlExpression(condition, data);
        }

        /// <summary>
        /// Process the <see cref="SelectModifier"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlQuery Transform(SelectModifier toTransform, T data)
        {
            var newInner = TransformSparqlQuery(toTransform.InnerQuery, data);

            if (newInner != toTransform.InnerQuery)
            {
                return new SelectModifier(newInner, toTransform.Variables);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="OrderByModifier"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlQuery Transform(OrderByModifier toTransform, T data)
        {
            var newInner = TransformSparqlQuery(toTransform.InnerQuery, data);

            if (newInner is OrderByModifier)
            {
                var innerOrderBy = (OrderByModifier)newInner;

                return new OrderByModifier(innerOrderBy.InnerQuery, innerOrderBy.InnerQuery.Variables, toTransform.Ordering.Union(innerOrderBy.Ordering).ToArray());
            }
            else if (newInner != toTransform.InnerQuery)
            {
                return new OrderByModifier(newInner, newInner.Variables, toTransform.Ordering);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="SliceModifier"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlQuery Transform(SliceModifier toTransform, T data)
        {
            var newInner = TransformSparqlQuery(toTransform.InnerQuery, data);

            if (newInner is SliceModifier)
            {
                var innerSlice = (SliceModifier)newInner;

                int? limit = innerSlice.Limit;
                int? offset = innerSlice.Offset;

                if (toTransform.Offset.HasValue)
                {
                    if (offset.HasValue)
                    {
                        offset = offset.Value + toTransform.Offset.Value;
                    }
                    else
                    {
                        offset = toTransform.Offset;
                    }
                }

                if (toTransform.Limit.HasValue)
                {
                    if (limit.HasValue)
                    {
                        limit = Math.Min(toTransform.Limit.Value, limit.Value);
                    }
                    else
                    {
                        limit = toTransform.Limit;
                    }
                }

                if (offset.HasValue && offset.Value == 0)
                {
                    offset = null;
                }

                return new SliceModifier(innerSlice.InnerQuery, innerSlice.InnerQuery.Variables, limit, offset);
            }
            else
            {
                int? limit = toTransform.Limit;
                int? offset = toTransform.Offset;

                bool changed = false;
                if (offset.HasValue && offset.Value == 0)
                {
                    offset = null;
                    changed = true;
                }

                if (!offset.HasValue && !limit.HasValue)
                {
                    return newInner;
                }
                else if (changed || newInner != toTransform.InnerQuery)
                {
                    return new SliceModifier(newInner, newInner.Variables, limit, offset);
                }
                else
                {
                    return toTransform;
                }
            }
        }

        /// <summary>
        /// Process the <see cref="EmptyPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(EmptyPattern toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(FilterPattern toTransform, T data)
        {
            var newInner = TransformGraphPattern(toTransform.InnerPattern, data);
            var newCondition = TransformSparqlCondition(toTransform.Condition, data);

            if (newInner is NotMatchingPattern)
            {
                return newInner;
            }
            else if (newInner != toTransform.InnerPattern)
            {
                return new FilterPattern(newInner, newCondition);
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
        protected override IGraphPattern Transform(GraphPattern toTransform, T data)
        {
            var newInner = TransformGraphPattern(toTransform.InnerPattern, data);

            if (newInner is NotMatchingPattern)
            {
                return newInner;
            }
            else if (newInner != toTransform.InnerPattern)
            {
                return new GraphPattern(newInner);
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
        protected override IGraphPattern Transform(JoinPattern toTransform, T data)
        {
            var newPatterns = new List<IGraphPattern>();
            bool differs = false;

            foreach (IGraphPattern oldPattern in toTransform.JoinedGraphPatterns)
            {
                var newPattern = TransformGraphPattern(oldPattern, data);

                if (newPattern is NotMatchingPattern)
                {
                    return new NotMatchingPattern();
                }
                else if (newPattern is EmptyPattern)
                {
                    differs = true;
                }
                else
                {
                    if (oldPattern != newPattern)
                    {
                        differs = true;
                    }

                    newPatterns.Add(newPattern);
                }
            }

            if (newPatterns.Count == 0)
            {
                return new EmptyPattern();
            }
            else if (newPatterns.Count == 1)
            {
                return newPatterns[0];
            }
            else if (differs)
            {
                return new JoinPattern(newPatterns);
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
        protected override IGraphPattern Transform(LeftJoinPattern toTransform, T data)
        {
            var newLeftOperand = TransformGraphPattern(toTransform.LeftOperand, data);
            var newRightOperand = TransformGraphPattern(toTransform.RightOperand, data);
            var newCondition = TransformSparqlCondition(toTransform.Condition, data);

            if (newLeftOperand is NotMatchingPattern)
            {
                return newLeftOperand;
            }
            else if (newCondition is BooleanFalseExpression)
            {
                return newLeftOperand;
            }
            else if (newRightOperand is NotMatchingPattern || newRightOperand is EmptyPattern)
            {
                return newLeftOperand;
            }

            if (newLeftOperand != toTransform.LeftOperand || newRightOperand != toTransform.RightOperand || newCondition != toTransform.Condition)
            {
                return new LeftJoinPattern(newLeftOperand, newRightOperand, newCondition);
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
        protected override IGraphPattern Transform(MinusPattern toTransform, T data)
        {
            var newLeftOperand = TransformGraphPattern(toTransform.LeftOperand, data);
            var newRightOperand = TransformGraphPattern(toTransform.RightOperand, data);

            if (newLeftOperand is NotMatchingPattern
                || newLeftOperand is EmptyPattern
                || newRightOperand is NotMatchingPattern
                || newRightOperand is EmptyPattern)
            {
                return newLeftOperand;
            }

            if (newLeftOperand != toTransform.LeftOperand
                || newRightOperand != toTransform.RightOperand)
            {
                return new MinusPattern(newLeftOperand, newRightOperand);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(TriplePattern toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(UnionPattern toTransform, T data)
        {
            var newPatterns = new List<IGraphPattern>();
            bool differs = false;

            foreach (IGraphPattern oldPattern in toTransform.UnionedGraphPatterns)
            {
                var newPattern = TransformGraphPattern(oldPattern, data);

                if (newPattern is NotMatchingPattern)
                {
                    differs = true;
                }
                else
                {
                    if (oldPattern != newPattern)
                    {
                        differs = true;
                    }

                    newPatterns.Add(newPattern);
                }
            }

            if (newPatterns.Count == 0)
            {
                return new NotMatchingPattern();
            }
            else if (newPatterns.Count == 1)
            {
                return newPatterns[0];
            }
            else if (differs)
            {
                return new UnionPattern(newPatterns);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="NotMatchingPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(NotMatchingPattern toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="RestrictedTriplePattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(RestrictedTriplePattern toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ExtendPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(ExtendPattern toTransform, T data)
        {
            var newInner = TransformGraphPattern(toTransform.InnerPattern, data);
            var newExpression = TransformSparqlExpression(toTransform.Expression, data);

            if (newInner is NotMatchingPattern)
            {
                return newInner;
            }
            else if (newInner != toTransform.InnerPattern || newExpression != toTransform.Expression)
            {
                return new ExtendPattern(newInner, toTransform.VariableName, newExpression);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="IsBoundExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(IsBoundExpression toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="BooleanTrueExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(BooleanTrueExpression toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="BooleanFalseExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(BooleanFalseExpression toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="NegationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(NegationExpression toTransform, T data)
        {
            var newInner = TransformSparqlCondition(toTransform.InnerCondition, data);

            if (newInner is BooleanTrueExpression)
            {
                return new BooleanFalseExpression();
            }
            else if (newInner is BooleanFalseExpression)
            {
                return new BooleanTrueExpression();
            }
            else if (newInner is ComparisonExpression)
            {
                var comparisonExpression = (ComparisonExpression)newInner;
                var left = comparisonExpression.LeftOperand;
                var right = comparisonExpression.RightOperand;

                switch (comparisonExpression.ComparisonType)
                {
                    case ComparisonTypes.GreaterThan:
                        return new ComparisonExpression(left, right, ComparisonTypes.LessOrEqualThan);
                    case ComparisonTypes.GreaterOrEqualThan:
                        return new ComparisonExpression(left, right, ComparisonTypes.LessThan);
                    case ComparisonTypes.LessThan:
                        return new ComparisonExpression(left, right, ComparisonTypes.GreaterOrEqualThan);
                    case ComparisonTypes.LessOrEqualThan:
                        return new ComparisonExpression(left, right, ComparisonTypes.GreaterThan);
                    case ComparisonTypes.EqualTo:
                        return new ComparisonExpression(left, right, ComparisonTypes.NotEqualTo);
                    case ComparisonTypes.NotEqualTo:
                        return new ComparisonExpression(left, right, ComparisonTypes.EqualTo);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (newInner != toTransform.InnerCondition)
            {
                return new NegationExpression(newInner);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="VariableExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(VariableExpression toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConjunctionExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(ConjunctionExpression toTransform, T data)
        {
            var conditions = new List<ISparqlCondition>();
            var changed = false;

            foreach (var toTransformInner in toTransform.Operands)
            {
                var newInner = TransformSparqlCondition(toTransformInner, data);

                if (newInner is BooleanTrueExpression)
                {
                    changed = true;
                }
                else if (newInner is BooleanFalseExpression)
                {
                    return newInner;
                }
                else if (newInner != toTransformInner)
                {
                    changed = true;
                    conditions.Add(newInner);
                }
                else
                {
                    conditions.Add(toTransformInner);
                }
            }

            if (conditions.Count == 0)
            {
                return new BooleanTrueExpression();
            }
            else if (conditions.Count == 1)
            {
                return conditions[0];
            }
            else if (changed)
            {
                return new ConjunctionExpression(conditions);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="ComparisonExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(ComparisonExpression toTransform, T data)
        {
            var newLeft = TransformSparqlExpression(toTransform.LeftOperand, data);
            var newRight = TransformSparqlExpression(toTransform.RightOperand, data);

            if (newLeft != toTransform.LeftOperand || newRight != toTransform.RightOperand)
            {
                return new ComparisonExpression(newLeft, newRight, toTransform.ComparisonType);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="NodeExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(NodeExpression toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="DisjunctionExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISparqlExpression Transform(DisjunctionExpression toTransform, T data)
        {
            var conditions = new List<ISparqlCondition>();
            var changed = false;

            foreach (var toTransformInner in toTransform.Operands)
            {
                var newInner = TransformSparqlCondition(toTransformInner, data);

                if (newInner is BooleanFalseExpression)
                {
                    changed = true;
                }
                else if (newInner is BooleanTrueExpression)
                {
                    return newInner;
                }
                else if (newInner != toTransformInner)
                {
                    changed = true;
                    conditions.Add(newInner);
                }
                else
                {
                    conditions.Add(toTransformInner);
                }
            }

            if (conditions.Count == 0)
            {
                return new BooleanFalseExpression();
            }
            else if (conditions.Count == 1)
            {
                return conditions[0];
            }
            else if (changed)
            {
                return new DisjunctionExpression(conditions);
            }
            else
            {
                return toTransform;
            }
        }
    }
}
