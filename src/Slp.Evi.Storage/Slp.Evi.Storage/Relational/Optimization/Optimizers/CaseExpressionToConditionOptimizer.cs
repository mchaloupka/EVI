using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.Optimization.Optimizers
{
    /// <summary>
    /// This class provides the ability to extract <see cref="CaseExpression"/> in
    /// <see cref="ComparisonCondition"/>.
    /// </summary>
    public class CaseExpressionToConditionOptimizer
        : BaseRelationalOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseExpressionToConditionOptimizer"/> class.
        /// </summary>
        public CaseExpressionToConditionOptimizer() 
            : base(new CaseExpressionToConditionOptimizerImplementation())
        { }

        /// <summary>
        /// Implementation for <see cref="CaseExpressionToConditionOptimizer"/>
        /// </summary>
        public class CaseExpressionToConditionOptimizerImplementation
            : BaseRelationalOptimizerImplementation<object>
        {
            /// <summary>
            /// Process the <see cref="ComparisonCondition"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IFilterCondition Transform(ComparisonCondition toTransform, OptimizationContext data)
            {
                return base.Transform(toTransform, data);
            }
        }
    }
}
