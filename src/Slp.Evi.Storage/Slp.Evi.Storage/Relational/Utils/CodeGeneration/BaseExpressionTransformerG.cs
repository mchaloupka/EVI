// This is generated code, do not edit!!!
using System;

using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Expressions;
namespace Slp.Evi.Storage.Relational.Utils.CodeGeneration
{
    /// <summary>
    /// Base generated transformer for <see cref="IExpressionVisitor" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    /// <typeparam name="TR">Type of the transformation result</typeparam>
    /// <typeparam name="T1">Type of the transformation result when processing <see cref="IAssignmentCondition" /></typeparam>
    /// <typeparam name="T2">Type of the transformation result when processing <see cref="ISourceCondition" /></typeparam>
    /// <typeparam name="T3">Type of the transformation result when processing <see cref="IFilterCondition" /></typeparam>
    /// <typeparam name="T4">Type of the transformation result when processing <see cref="ICalculusSource" /></typeparam>
    public abstract class BaseExpressionTransformerG<T, TR, T1, T2, T3, T4>
        : BaseAssignmentConditionTransformerG<T, T1, T2, T3, T4>, IExpressionVisitor
    {
        /// <summary>
        /// Transforms the <see cref="IExpression" />.
        /// </summary>
        /// <param name="instance">The instance to transform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public TR TransformExpression(IExpression instance, T data)
        {
            return (TR)instance.Accept(this, data);
        }
        /// <summary>
        /// Decides whether we should use standard or fallback transformation for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should process standardly, <c>false</c> the fallback should be used.</returns>
        protected virtual bool CommonShouldTransform(IExpression toTransform, T data)
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
        protected virtual TR CommonPostTransform(TR transformed, IExpression toTransform, T data)
        {
            return transformed;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR CommonFallbackTransform(IExpression toTransform, T data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="ColumnExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ColumnExpression toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="ColumnExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(ColumnExpression toVisit, T data) 
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
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ColumnExpression toTransform, T data);

        /// <summary>
        /// Pre-process for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(ColumnExpression toTransform, T data)
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
        protected virtual TR PostTransform(TR transformed, ColumnExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(ColumnExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="ConcatenationExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConcatenationExpression toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="ConcatenationExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(ConcatenationExpression toVisit, T data) 
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
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ConcatenationExpression toTransform, T data);

        /// <summary>
        /// Pre-process for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(ConcatenationExpression toTransform, T data)
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
        protected virtual TR PostTransform(TR transformed, ConcatenationExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(ConcatenationExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="ConstantExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConstantExpression toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="ConstantExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(ConstantExpression toVisit, T data) 
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
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ConstantExpression toTransform, T data);

        /// <summary>
        /// Pre-process for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(ConstantExpression toTransform, T data)
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
        protected virtual TR PostTransform(TR transformed, ConstantExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(ConstantExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="CaseExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CaseExpression toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="CaseExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(CaseExpression toVisit, T data) 
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
        /// Process the <see cref="CaseExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(CaseExpression toTransform, T data);

        /// <summary>
        /// Pre-process for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(CaseExpression toTransform, T data)
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
        protected virtual TR PostTransform(TR transformed, CaseExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(CaseExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="CoalesceExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CoalesceExpression toVisit, object data)
        {
            return ProcessVisit(toVisit, (T)data);
        }

        /// <summary>
        /// Processes the visit of <see cref="CoalesceExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        protected virtual TR ProcessVisit(CoalesceExpression toVisit, T data) 
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
        /// Process the <see cref="CoalesceExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(CoalesceExpression toTransform, T data);

        /// <summary>
        /// Pre-process for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(CoalesceExpression toTransform, T data)
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
        protected virtual TR PostTransform(TR transformed, CoalesceExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(CoalesceExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

    }
}
