using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Sparql.Algebra;
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
        : BaseGraphPatternTransformerG<T, IGraphPattern, ISparqlQuery>
    {
        /// <summary>
        /// Transforms the <see cref="ISparqlQuery" />.
        /// </summary>
        /// <param name="instance">The instance to tranform.</param>
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

            if (newInner is NotMatchingPattern)
            {
                return newInner;
            }
            else if (newInner != toTransform.InnerPattern)
            {
                return new FilterPattern(newInner);
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
                return new FilterPattern(newInner);
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

            if (newLeftOperand is NotMatchingPattern)
            {
                return newLeftOperand;
            }
            else if (newRightOperand is NotMatchingPattern || newRightOperand is EmptyPattern)
            {
                return newLeftOperand;
            }

            if (newLeftOperand != toTransform.LeftOperand || newRightOperand != toTransform.RightOperand)
            {
                return new LeftJoinPattern(newLeftOperand, newRightOperand);
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
    }
}
