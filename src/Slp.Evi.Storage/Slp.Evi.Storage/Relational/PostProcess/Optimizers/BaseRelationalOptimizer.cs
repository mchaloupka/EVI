using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Utils;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers
{
    /// <summary>
    /// The base class for relational optimizers
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public abstract class BaseRelationalOptimizer<T>
        : BaseRelationalTransformer<BaseRelationalOptimizer<T>.OptimizationContext>,
        IRelationalPostProcess
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
        protected BaseRelationalOptimizerImplementation<T> OptimizerImplementation => _optimizerImplementation;

        /// <summary>
        /// The optimization context
        /// </summary>
        public class OptimizationContext
        {
            /// <summary>
            /// Gets or sets the query context.
            /// </summary>
            public IQueryContext Context { get; set; }

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
        public virtual RelationalQuery Process(RelationalQuery query, IQueryContext context)
        {
            var optimizationContext = CreateInitialContext(query, context);
            var modifiedModel = (ICalculusSource) query.Model.Accept(this, optimizationContext);

            if (modifiedModel != query.Model)
            {
                return OptimizeRelationalQuery(new RelationalQuery(modifiedModel, query.ValueBinders), optimizationContext);
            }
            else
            {
                return OptimizeRelationalQuery(query, optimizationContext);
            }
        }

        /// <summary>
        /// Optimizes the relational query.
        /// </summary>
        /// <param name="relationalQuery">The relational query.</param>
        /// <param name="optimizationContext">The optimization context.</param>
        protected virtual RelationalQuery OptimizeRelationalQuery(RelationalQuery relationalQuery, OptimizationContext optimizationContext)
        {
            return relationalQuery;
        }

        /// <summary>
        /// Creates the initial context
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="context">The context</param>
        /// <returns></returns>
        private OptimizationContext CreateInitialContext(RelationalQuery query, IQueryContext context)
        {
            return new OptimizationContext()
            {
                Context = context,
                Data = CreateInitialData(query, context)
            };
        }

        /// <summary>
        /// Creates the initial data.
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="context">The context</param>
        protected virtual T CreateInitialData(RelationalQuery query, IQueryContext context)
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
            return base.CommonPostTransform(_optimizerImplementation.TransformAssignmentCondition(transformed, data), toTransform, data);
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
            return base.CommonPostTransform(_optimizerImplementation.TransformExpression(transformed, data), toTransform, data);
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
            return base.CommonPostTransform(_optimizerImplementation.TransformSourceCondition(transformed, data), toTransform, data);
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
            return base.CommonPostTransform(_optimizerImplementation.TransformCalculusSource(transformed, data), toTransform, data);
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
            return base.CommonPostTransform(_optimizerImplementation.TransformFilterCondition(transformed, data), toTransform, data);
        }
    }
}
