using Slp.r2rml4net.Storage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Visitor interface for SPARQL patterns
    /// </summary>
    public interface IPatternVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="EmptyPattern"/>
        /// </summary>
        /// <param name="emptyPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(EmptyPattern emptyPattern, object data);

        /// <summary>
        /// Visits <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="filterPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(FilterPattern filterPattern, object data);

        /// <summary>
        /// Visits <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="graphPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(GraphPattern graphPattern, object data);

        /// <summary>
        /// Visits <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="joinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(JoinPattern joinPattern, object data);

        /// <summary>
        /// Visits <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="leftJoinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(LeftJoinPattern leftJoinPattern, object data);

        /// <summary>
        /// Visits <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="minusPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(MinusPattern minusPattern, object data);

        /// <summary>
        /// Visits <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="triplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(TriplePattern triplePattern, object data);

        /// <summary>
        /// Visits <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="unionPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(UnionPattern unionPattern, object data);
    }
}
