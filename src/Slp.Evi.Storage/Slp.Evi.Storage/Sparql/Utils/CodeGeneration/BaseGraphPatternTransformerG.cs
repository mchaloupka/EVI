// This is generated code, do not edit!!!

using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
namespace Slp.Evi.Storage.Sparql.Utils.CodeGeneration
{
    /// <summary>
    /// Base generated transformer for <see cref="IGraphPatternVisitor" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    /// <typeparam name="TR">Type of the transformation result</typeparam>
    /// <typeparam name="T1">Type of the transformation result when processing <see cref="IModifier" /></typeparam>
    public abstract class BaseGraphPatternTransformerG<T, TR, T1>
        : BaseModifierTransformerG<T, T1>, IGraphPatternVisitor
    {
        /// <summary>
        /// Transforms the <see cref="IGraphPattern" />.
        /// </summary>
        /// <param name="instance">The instance to transform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public TR TransformGraphPattern(IGraphPattern instance, T data)
        {
            return (TR)instance.Accept(this, data);
        }
        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR CommonPostTransform(TR transformed, IGraphPattern toTransform, T data)
        {
            return transformed;
        }

        /// <summary>
        /// Visits <see cref="EmptyPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="EmptyPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(EmptyPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="EmptyPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(EmptyPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, EmptyPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="FilterPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(FilterPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="FilterPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(FilterPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="FilterPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(FilterPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, FilterPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="NotMatchingPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NotMatchingPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="NotMatchingPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(NotMatchingPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="NotMatchingPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(NotMatchingPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, NotMatchingPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="GraphPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(GraphPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="GraphPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(GraphPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(GraphPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, GraphPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="JoinPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(JoinPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="JoinPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(JoinPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="JoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(JoinPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, JoinPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="LeftJoinPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(LeftJoinPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="LeftJoinPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(LeftJoinPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(LeftJoinPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, LeftJoinPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="MinusPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(MinusPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="MinusPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(MinusPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(MinusPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, MinusPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="TriplePattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(TriplePattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="TriplePattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(TriplePattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(TriplePattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, TriplePattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="UnionPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="UnionPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(UnionPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(UnionPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, UnionPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="RestrictedTriplePattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(RestrictedTriplePattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="RestrictedTriplePattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(RestrictedTriplePattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="RestrictedTriplePattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(RestrictedTriplePattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, RestrictedTriplePattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="ExtendPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ExtendPattern toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="ExtendPattern" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(ExtendPattern toVisit, T data)
        {
            var transformed = Transform(toVisit, data);
            return PostTransform(transformed, toVisit, data);
        }

        /// <summary>
        /// Process the <see cref="ExtendPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ExtendPattern toTransform, T data);

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, ExtendPattern toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

    }
}
