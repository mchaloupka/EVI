// This is generated code, do not edit!!!
using System;

using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
namespace Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration
{
    /// <summary>
    /// Base generated transformer for <see cref="IFilterConditionVisitor" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    /// <typeparam name="TR">Type of the transformation result</typeparam>
    /// <typeparam name="T1">Type of the transformation result when processing <see cref="ICalculusSource" /></typeparam>
    public abstract class BaseFilterConditionTransformerG<T, TR, T1>
        : BaseSourceTransformerG<T, T1>, IFilterConditionVisitor
    {
        /// <summary>
        /// Transforms the <see cref="IFilterCondition" />.
        /// </summary>
        /// <param name="instance">The instance to tranform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public TR Transform(IFilterCondition instance, T data)
        {
            return (TR)instance.Accept(this, data);
        }
        /// <summary>
        /// Decides whether we should use standard or fallback transformation for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should process standardly, <c>false</c> the fallback should be used.</returns>
        protected virtual bool CommonShouldTransform(IFilterCondition toTransform, T data)
        {
            return true;
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR CommonPostTransform(TR transformed, IFilterCondition toTransform, T data)
        {
            return transformed;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR CommonFallbackTransform(IFilterCondition toTransform, T data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="AlwaysFalseCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(AlwaysFalseCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(AlwaysFalseCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(AlwaysFalseCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, AlwaysFalseCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(AlwaysFalseCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="AlwaysTrueCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(AlwaysTrueCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(AlwaysTrueCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(AlwaysTrueCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, AlwaysTrueCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(AlwaysTrueCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="ConjunctionCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConjunctionCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ConjunctionCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(ConjunctionCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, ConjunctionCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(ConjunctionCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="DisjunctionCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(DisjunctionCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(DisjunctionCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(DisjunctionCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, DisjunctionCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(DisjunctionCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="EqualExpressionCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EqualExpressionCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(EqualExpressionCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(EqualExpressionCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, EqualExpressionCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(EqualExpressionCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="EqualVariablesCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EqualVariablesCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(EqualVariablesCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(EqualVariablesCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, EqualVariablesCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(EqualVariablesCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="IsNullCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(IsNullCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(IsNullCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(IsNullCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, IsNullCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(IsNullCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="NegationCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NegationCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(NegationCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(NegationCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, NegationCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(NegationCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

    }
}
