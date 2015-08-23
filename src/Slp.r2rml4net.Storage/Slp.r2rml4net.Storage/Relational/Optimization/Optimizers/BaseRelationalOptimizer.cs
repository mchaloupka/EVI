using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Storage.Relational.Utils;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers
{
    /// <summary>
    /// The base class for relational optimizers
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public abstract class BaseRelationalOptimizer<T>
        : BaseRelationalTransformer<BaseRelationalOptimizer<T>.OptimizationContext>,
        IRelationalOptimizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRelationalOptimizer{T}"/> class.
        /// </summary>
        /// <param name="optimizerImplementation">The optimizer implementation.</param>
        protected BaseRelationalOptimizer(BaseRelationalOptimizerImplementation<T> optimizerImplementation)
        {
            _optimizerImplementation = optimizerImplementation;
        } 

        /// <summary>
        /// The optimizer implementation
        /// </summary>
        private readonly BaseRelationalOptimizerImplementation<T> _optimizerImplementation;

        /// <summary>
        /// Gets the optimizer implementation.
        /// </summary>
        /// <value>The optimizer implementation.</value>
        protected BaseRelationalOptimizerImplementation<T> OptimizerImplementation
        {
            get
            {
                return _optimizerImplementation;
            }
        }

        /// <summary>
        /// The optimization context
        /// </summary>
        public class OptimizationContext
        {
            /// <summary>
            /// Gets or sets the query context.
            /// </summary>
            public QueryContext Context { get; set; }

            /// <summary>
            /// Gets or sets the data.
            /// </summary>
            public T Data { get; set; }
        }

        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        public virtual RelationalQuery Optimize(RelationalQuery query, QueryContext context)
        {
            var modifiedModel = (CalculusModel)Visit(query.Model, new OptimizationContext()
            {
                Context = context,
                Data = CreateInitialData()
            });

            if (modifiedModel != query.Model)
            {
                return new RelationalQuery(modifiedModel, query.ValueBinders);
            }
            else
            {
                return query;
            }
        }

        /// <summary>
        /// Creates the initial data.
        /// </summary>
        protected virtual T CreateInitialData()
        {
            return default(T);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected override IAssignmentCondition CommonPostTransform(IAssignmentCondition transformed, IAssignmentCondition toTransform,
            OptimizationContext data)
        {
            return base.CommonPostTransform(_optimizerImplementation.Transform(transformed, data), toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected override IExpression CommonPostTransform(IExpression transformed, IExpression toTransform, OptimizationContext data)
        {
            return base.CommonPostTransform(_optimizerImplementation.Transform(transformed, data), toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected override ISourceCondition CommonPostTransform(ISourceCondition transformed, ISourceCondition toTransform,
            OptimizationContext data)
        {
            return base.CommonPostTransform(_optimizerImplementation.Transform(transformed, data), toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected override ICalculusSource CommonPostTransform(ICalculusSource transformed, ICalculusSource toTransform, OptimizationContext data)
        {
            return base.CommonPostTransform(_optimizerImplementation.Transform(transformed, data), toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected override IFilterCondition CommonPostTransform(IFilterCondition transformed, IFilterCondition toTransform,
            OptimizationContext data)
        {
            return base.CommonPostTransform(_optimizerImplementation.Transform(transformed, data), toTransform, data);
        }
    }
}
