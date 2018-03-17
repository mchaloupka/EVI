// This is generated code, do not edit!!!
using System;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;

namespace Slp.Evi.Storage.Relational.Builder.CodeGeneration
{
    /// <summary>
    /// Relational builder
    /// </summary>
    public abstract class BaseRelationalBuilder
        : IModifierVisitor, IGraphPatternVisitor
    {
        /// <summary>
        /// Visits <see cref="EmptyPattern" />
        /// </summary>
        /// <param name="emptyPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyPattern emptyPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(emptyPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(EmptyPattern emptyPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="FilterPattern" />
        /// </summary>
        /// <param name="filterPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(FilterPattern filterPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(filterPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(FilterPattern filterPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="NotMatchingPattern" />
        /// </summary>
        /// <param name="notMatchingPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NotMatchingPattern notMatchingPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(notMatchingPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(NotMatchingPattern notMatchingPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="GraphPattern" />
        /// </summary>
        /// <param name="graphPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(GraphPattern graphPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(graphPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(GraphPattern graphPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="JoinPattern" />
        /// </summary>
        /// <param name="joinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(JoinPattern joinPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(joinPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(JoinPattern joinPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="LeftJoinPattern" />
        /// </summary>
        /// <param name="leftJoinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(LeftJoinPattern leftJoinPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(leftJoinPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(LeftJoinPattern leftJoinPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="MinusPattern" />
        /// </summary>
        /// <param name="minusPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(MinusPattern minusPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(minusPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(MinusPattern minusPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="TriplePattern" />
        /// </summary>
        /// <param name="triplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(TriplePattern triplePattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(triplePattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(TriplePattern triplePattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="UnionPattern" />
        /// </summary>
        /// <param name="unionPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionPattern unionPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(unionPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(UnionPattern unionPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="RestrictedTriplePattern" />
        /// </summary>
        /// <param name="restrictedTriplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(RestrictedTriplePattern restrictedTriplePattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(restrictedTriplePattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(RestrictedTriplePattern restrictedTriplePattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="ExtendPattern" />
        /// </summary>
        /// <param name="extendPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExtendPattern extendPattern, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(extendPattern, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(ExtendPattern extendPattern, IQueryContext context);
        /// <summary>
        /// Visits <see cref="SelectModifier" />
        /// </summary>
        /// <param name="selectModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SelectModifier selectModifier, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(selectModifier, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(SelectModifier selectModifier, IQueryContext context);
        /// <summary>
        /// Visits <see cref="OrderByModifier" />
        /// </summary>
        /// <param name="orderByModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(OrderByModifier orderByModifier, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(orderByModifier, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(OrderByModifier orderByModifier, IQueryContext context);
        /// <summary>
        /// Visits <see cref="SliceModifier" />
        /// </summary>
        /// <param name="sliceModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SliceModifier sliceModifier, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(sliceModifier, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(SliceModifier sliceModifier, IQueryContext context);
        /// <summary>
        /// Visits <see cref="DistinctModifier" />
        /// </summary>
        /// <param name="distinctModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(DistinctModifier distinctModifier, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(distinctModifier, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(DistinctModifier distinctModifier, IQueryContext context);
    }
}

