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
    public abstract class BaseSparqlTransformer
        : IPatternVisitor
    {
        /// <summary>
        /// Visits <see cref="EmptyPattern"/>
        /// </summary>
        /// <param name="emptyPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyPattern emptyPattern, object data)
        {
            return emptyPattern;
        }

        /// <summary>
        /// Visits <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="filterPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(FilterPattern filterPattern, object data)
        {
            return filterPattern;
        }

        /// <summary>
        /// Visits <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="graphPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(GraphPattern graphPattern, object data)
        {
            return graphPattern;
        }

        /// <summary>
        /// Visits <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="joinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(JoinPattern joinPattern, object data)
        {
            return joinPattern;
        }

        /// <summary>
        /// Visits <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="leftJoinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(LeftJoinPattern leftJoinPattern, object data)
        {
            return leftJoinPattern;
        }

        /// <summary>
        /// Visits <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="minusPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(MinusPattern minusPattern, object data)
        {
            return minusPattern;
        }

        /// <summary>
        /// Visits <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="triplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(TriplePattern triplePattern, object data)
        {
            return triplePattern;
        }

        /// <summary>
        /// Visits <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="unionPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionPattern unionPattern, object data)
        {
            return unionPattern;
        }
    }
}
