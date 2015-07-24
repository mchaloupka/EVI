using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Storage.Relational.Utils;

namespace Slp.r2rml4net.Storage.Relational.Optimization
{
    /// <summary>
    /// The base class for relational optimizers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseRelationalOptimizer<T>
        : BaseRelationalTransformer<BaseRelationalOptimizer<T>.OptimizationContext>,
        IRelationalOptimizer
    {
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
            var modifiedModel = (CalculusModel)Transform(query.Model, new OptimizationContext()
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
    }
}
