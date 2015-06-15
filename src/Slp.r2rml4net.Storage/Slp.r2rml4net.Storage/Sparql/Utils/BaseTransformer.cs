using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Modifiers;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Utils
{
    /// <summary>
    /// Base class for SPARQL transformations
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public abstract class BaseSparqlTransformer<T>
        : IPatternVisitor, IModifierVisitor
    {
        #region Virtual transformation methods to be overriden
        /// <summary>
        /// Process the <see cref="EmptyPattern"/>
        /// </summary>
        /// <param name="emptyPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(EmptyPattern emptyPattern, T data)
        {
            return emptyPattern;
        }

        /// <summary>
        /// Process the <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="filterPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(FilterPattern filterPattern, T data)
        {
            return filterPattern;
        }

        /// <summary>
        /// Process the <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="graphPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(GraphPattern graphPattern, T data)
        {
            return graphPattern;
        }

        /// <summary>
        /// Process the <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="joinPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(JoinPattern joinPattern, T data)
        {
            return joinPattern;
        }

        /// <summary>
        /// Process the <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="leftJoinPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(LeftJoinPattern leftJoinPattern, T data)
        {
            return leftJoinPattern;
        }

        /// <summary>
        /// Process the <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="minusPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(MinusPattern minusPattern, T data)
        {
            return minusPattern;
        }

        /// <summary>
        /// Process the <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="triplePattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(TriplePattern triplePattern, T data)
        {
            return triplePattern;
        }

        /// <summary>
        /// Process the <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="unionPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(UnionPattern unionPattern, T data)
        {
            return unionPattern;
        }

        /// <summary>
        /// Process the <see cref="NotMatchingPattern"/>
        /// </summary>
        /// <param name="notMatchingPattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual IGraphPattern Process(NotMatchingPattern notMatchingPattern, T data)
        {
            return notMatchingPattern;
        }

        /// <summary>
        /// Process the <see cref="SelectModifier"/>
        /// </summary>
        /// <param name="selectModifier">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        private object Process(SelectModifier selectModifier, T data)
        {
            return selectModifier;
        }
        #endregion

        #region IPatternVisitor
        /// <summary>
        /// Visits <see cref="EmptyPattern"/>
        /// </summary>
        /// <param name="emptyPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyPattern emptyPattern, object data)
        {
            return Process(emptyPattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="filterPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(FilterPattern filterPattern, object data)
        {
            var newInner = (IGraphPattern)filterPattern.InnerPattern.Accept(this, data);

            if(newInner != filterPattern.InnerPattern)
            {
                filterPattern = new FilterPattern(newInner);
            }

            if(filterPattern.InnerPattern is NotMatchingPattern)
            {
                return new NotMatchingPattern(filterPattern.Variables);
            }

            return Process(filterPattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="graphPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(GraphPattern graphPattern, object data)
        {
            var newInner = (IGraphPattern)graphPattern.InnerPattern.Accept(this, data);

            if (newInner != graphPattern.InnerPattern)
            {
                graphPattern = new GraphPattern(newInner);
            }

            if(graphPattern.InnerPattern is NotMatchingPattern)
            {
                return new NotMatchingPattern(graphPattern.Variables);
            }

            return Process(graphPattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="joinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(JoinPattern joinPattern, object data)
        {
            var oldPatterns = joinPattern.JoinedGraphPatterns.ToList();
            var newPatterns = joinPattern.JoinedGraphPatterns
                .Select(x => x.Accept(this, data)).Cast<IGraphPattern>()
                .ToList();

            bool differs = false;

            for (int i = 0; i < oldPatterns.Count; i++)
            {
                if (oldPatterns[i] != newPatterns[i])
                {
                    differs = true;
                }
            }

            if(newPatterns.OfType<NotMatchingPattern>().Any())
            {
                return new NotMatchingPattern(joinPattern.Variables);
            }

            if (newPatterns.OfType<EmptyPattern>().Any())
            {
                newPatterns = newPatterns
                    .Where(x => !(x is EmptyPattern))
                    .ToList();

                differs = true;
            }

            if(differs)
            {
                if(newPatterns.Count == 0)
                {
                    return new EmptyPattern();
                }
                else if(newPatterns.Count == 1)
                {
                    return newPatterns[0];
                }
                else
                {
                    joinPattern = new JoinPattern(newPatterns);
                }
            }

            return Process(joinPattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="leftJoinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(LeftJoinPattern leftJoinPattern, object data)
        {
            var newLeftOperand = (IGraphPattern)leftJoinPattern.LeftOperand.Accept(this, data);
            var newRightOperand = (IGraphPattern)leftJoinPattern.RightOperand.Accept(this, data);

            if (newLeftOperand is NotMatchingPattern)
            {
                return new NotMatchingPattern(leftJoinPattern.Variables);
            }
            else if(newRightOperand is NotMatchingPattern)
            {
                return newLeftOperand;
            }

            if(newLeftOperand != leftJoinPattern.LeftOperand
                || newRightOperand != leftJoinPattern.RightOperand)
            {
                leftJoinPattern = new LeftJoinPattern(newLeftOperand, newRightOperand);
            }

            return Process(leftJoinPattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="minusPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(MinusPattern minusPattern, object data)
        {
            var newLeftOperand = (IGraphPattern)minusPattern.LeftOperand.Accept(this, data);
            var newRightOperand = (IGraphPattern)minusPattern.RightOperand.Accept(this, data);

            if(newLeftOperand is NotMatchingPattern 
                || newLeftOperand is EmptyPattern
                || newRightOperand is NotMatchingPattern
                || newRightOperand is EmptyPattern)
            {
                return newLeftOperand;
            }

            if(newLeftOperand != minusPattern.LeftOperand
                || newRightOperand != minusPattern.RightOperand)
            {
                minusPattern = new MinusPattern(newLeftOperand, newRightOperand);
            }

            return Process(minusPattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="triplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(TriplePattern triplePattern, object data)
        {
            return Process(triplePattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="unionPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionPattern unionPattern, object data)
        {
            var oldPatterns = unionPattern.UnionedGraphPatterns.ToList();
            var newPatterns = unionPattern.UnionedGraphPatterns
                .Select(x => x.Accept(this, data)).Cast<IGraphPattern>()
                .ToList();

            bool differs = false;

            for (int i = 0; i < oldPatterns.Count; i++)
            {
                if (oldPatterns[i] != newPatterns[i])
                {
                    differs = true;
                }
            }

            if (newPatterns.OfType<NotMatchingPattern>().Any())
            {
                newPatterns = newPatterns
                    .Where(x => !(x is NotMatchingPattern))
                    .ToList();

                differs = true;
            }

            if (differs)
            {
                if (newPatterns.Count == 0)
                {
                    return new NotMatchingPattern(unionPattern.Variables);
                }
                else if (newPatterns.Count == 1)
                {
                    return newPatterns[0];
                }
                else
                {
                    unionPattern = new UnionPattern(newPatterns);
                }
            }

            return Process(unionPattern, (T)data);
        }

        /// <summary>
        /// Visits <see cref="NotMatchingPattern"/>
        /// </summary>
        /// <param name="notMatchingPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NotMatchingPattern notMatchingPattern, object data)
        {
            return Process(notMatchingPattern, (T)data);
        }
        #endregion

        #region IModifierVisitor
        /// <summary>
        /// Visits <see cref="SelectModifier"/>
        /// </summary>
        /// <param name="selectModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SelectModifier selectModifier, object data)
        {
            return Process(selectModifier, (T)data);
        }
        #endregion
    }
}
