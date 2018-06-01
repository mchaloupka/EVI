// This is generated code, do not edit!!!

using Microsoft.Extensions.Logging;
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
        /// The logger
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRelationalBuilder"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected BaseRelationalBuilder(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public object Visit(EmptyPattern emptyPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(emptyPattern, context);

            context.DebugLogging.LogTransformation(_logger, emptyPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="EmptyPattern" />.
        /// </summary>
        /// <param name="emptyPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(EmptyPattern emptyPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(FilterPattern filterPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(filterPattern, context);

            context.DebugLogging.LogTransformation(_logger, filterPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="FilterPattern" />.
        /// </summary>
        /// <param name="filterPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(FilterPattern filterPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(NotMatchingPattern notMatchingPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(notMatchingPattern, context);

            context.DebugLogging.LogTransformation(_logger, notMatchingPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="NotMatchingPattern" />.
        /// </summary>
        /// <param name="notMatchingPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(NotMatchingPattern notMatchingPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(GraphPattern graphPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(graphPattern, context);

            context.DebugLogging.LogTransformation(_logger, graphPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="GraphPattern" />.
        /// </summary>
        /// <param name="graphPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(GraphPattern graphPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(JoinPattern joinPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(joinPattern, context);

            context.DebugLogging.LogTransformation(_logger, joinPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="JoinPattern" />.
        /// </summary>
        /// <param name="joinPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(JoinPattern joinPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(LeftJoinPattern leftJoinPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(leftJoinPattern, context);

            context.DebugLogging.LogTransformation(_logger, leftJoinPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="LeftJoinPattern" />.
        /// </summary>
        /// <param name="leftJoinPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(LeftJoinPattern leftJoinPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(MinusPattern minusPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(minusPattern, context);

            context.DebugLogging.LogTransformation(_logger, minusPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="MinusPattern" />.
        /// </summary>
        /// <param name="minusPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(MinusPattern minusPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(TriplePattern triplePattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(triplePattern, context);

            context.DebugLogging.LogTransformation(_logger, triplePattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="TriplePattern" />.
        /// </summary>
        /// <param name="triplePattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(TriplePattern triplePattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(UnionPattern unionPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(unionPattern, context);

            context.DebugLogging.LogTransformation(_logger, unionPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="UnionPattern" />.
        /// </summary>
        /// <param name="unionPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(UnionPattern unionPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(RestrictedTriplePattern restrictedTriplePattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(restrictedTriplePattern, context);

            context.DebugLogging.LogTransformation(_logger, restrictedTriplePattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="RestrictedTriplePattern" />.
        /// </summary>
        /// <param name="restrictedTriplePattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(RestrictedTriplePattern restrictedTriplePattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(ExtendPattern extendPattern, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(extendPattern, context);

            context.DebugLogging.LogTransformation(_logger, extendPattern, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="ExtendPattern" />.
        /// </summary>
        /// <param name="extendPattern">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(ExtendPattern extendPattern, IQueryContext context);

        /// <inheritdoc />
        public object Visit(SelectModifier selectModifier, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(selectModifier, context);

            context.DebugLogging.LogTransformation(_logger, selectModifier, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="SelectModifier" />.
        /// </summary>
        /// <param name="selectModifier">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(SelectModifier selectModifier, IQueryContext context);

        /// <inheritdoc />
        public object Visit(OrderByModifier orderByModifier, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(orderByModifier, context);

            context.DebugLogging.LogTransformation(_logger, orderByModifier, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="OrderByModifier" />.
        /// </summary>
        /// <param name="orderByModifier">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(OrderByModifier orderByModifier, IQueryContext context);

        /// <inheritdoc />
        public object Visit(SliceModifier sliceModifier, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(sliceModifier, context);

            context.DebugLogging.LogTransformation(_logger, sliceModifier, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="SliceModifier" />.
        /// </summary>
        /// <param name="sliceModifier">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(SliceModifier sliceModifier, IQueryContext context);

        /// <inheritdoc />
        public object Visit(DistinctModifier distinctModifier, object data)
        {
            var context = (IQueryContext) data;
            var transformed = Transform(distinctModifier, context);

            context.DebugLogging.LogTransformation(_logger, distinctModifier, transformed);

            return context.QueryPostProcesses.PostProcess(transformed);
        }

        /// <summary>
        /// Transforms the specified <see cref="DistinctModifier" />.
        /// </summary>
        /// <param name="distinctModifier">The instance to transform.</param>
        /// <param name="context">The query context.</param>
        protected abstract RelationalQuery Transform(DistinctModifier distinctModifier, IQueryContext context);

    }
}

