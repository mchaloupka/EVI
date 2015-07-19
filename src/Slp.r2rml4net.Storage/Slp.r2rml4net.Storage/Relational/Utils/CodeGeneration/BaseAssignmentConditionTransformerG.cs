// This is generated code, do not edit!!!

using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;

namespace Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration
{
    /// <summary>
    /// Base generated transformer for <see cref="IAssignmentConditionVisitor" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    /// <typeparam name="TR">Type of the transformation result</typeparam>
    /// <typeparam name="T1">Type of the transformation result when processing <see cref="ISourceCondition" /></typeparam>
    /// <typeparam name="T2">Type of the transformation result when processing <see cref="IFilterCondition" /></typeparam>
    /// <typeparam name="T3">Type of the transformation result when processing <see cref="ICalculusSource" /></typeparam>
    public abstract class BaseAssignmentConditionTransformerG<T, TR, T1, T2, T3>
        : BaseSourceConditionTransformerG<T, T1, T2, T3>, IAssignmentConditionVisitor
    {
        /// <summary>
        /// Transforms the <see cref="IAssignmentCondition" />.
        /// </summary>
        /// <param name="instance">The instance to tranform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public TR Transform(IAssignmentCondition instance, T data)
        {
            return (TR)instance.Accept(this, data);
        }
        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR CommonPostTransform(TR transformed, IAssignmentCondition toTransform, T data)
        {
            return transformed;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected abstract TR CommonFallbackTransform(IAssignmentCondition toTransform, T data);

    }
}
