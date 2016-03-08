// This is generated code, do not edit!!!
using System;

using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;

namespace Slp.Evi.Storage.Sparql.Utils.CodeGeneration
{
    /// <summary>
    /// Base generated transformer for <see cref="IModifierVisitor" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    /// <typeparam name="TR">Type of the transformation result</typeparam>
    public abstract class BaseModifierTransformerG<T, TR>
        : IModifierVisitor
    {
        /// <summary>
        /// Transforms the <see cref="IModifier" />.
        /// </summary>
        /// <param name="instance">The instance to transform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public TR TransformModifier(IModifier instance, T data)
        {
            return (TR)instance.Accept(this, data);
        }
        /// <summary>
        /// Decides whether we should use standard or fallback transformation for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should process standardly, <c>false</c> the fallback should be used.</returns>
        protected virtual bool CommonShouldTransform(IModifier toTransform, T data)
        {
            return true;
        }

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR CommonPostTransform(TR transformed, IModifier toTransform, T data)
        {
            return transformed;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR CommonFallbackTransform(IModifier toTransform, T data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="SelectModifier" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SelectModifier toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="SelectModifier" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(SelectModifier toVisit, T data) 
        {
            if(ShouldTransform(toVisit, data))
            {
                var transformed = Transform(toVisit, data);
                return PostTransform(transformed, toVisit, data);
            }
            else
            {
                return FallbackTransform(toVisit, data);
            }
        }

        /// <summary>
        /// Process the <see cref="SelectModifier"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(SelectModifier toTransform, T data);

        /// <summary>
        /// Pre-process for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(SelectModifier toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Post-process for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The post-processed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, SelectModifier toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(SelectModifier toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

    }
}
