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
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(EmptyPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="FilterPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(FilterPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(FilterPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="NotMatchingPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NotMatchingPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(NotMatchingPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="GraphPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(GraphPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(GraphPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="JoinPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(JoinPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(JoinPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="LeftJoinPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(LeftJoinPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(LeftJoinPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="MinusPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(MinusPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(MinusPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="TriplePattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(TriplePattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(TriplePattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="UnionPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(UnionPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="RestrictedTriplePattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(RestrictedTriplePattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(RestrictedTriplePattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="ExtendPattern" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExtendPattern instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(ExtendPattern instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="SelectModifier" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SelectModifier instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(SelectModifier instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="OrderByModifier" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(OrderByModifier instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(OrderByModifier instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="SliceModifier" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SliceModifier instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(SliceModifier instance, IQueryContext context);
        /// <summary>
        /// Visits <see cref="DistinctModifier" />
        /// </summary>
        /// <param name="instance">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(DistinctModifier instance, object data)
        {
            return ((IQueryContext) data).QueryPostProcesses.PostProcess(Transform(instance, (IQueryContext) data));
        }

        protected abstract RelationalQuery Transform(DistinctModifier instance, IQueryContext context);
    }
}

