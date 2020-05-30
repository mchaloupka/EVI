using Microsoft.Extensions.Logging;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers
{
    /// <summary>
    /// Optimization for comparisons where a numeric column is on one side and string literal on the other
    /// </summary>
    public class NumericComparisonOptimizer
        : BaseRelationalOptimizer<object>
    {
        /// <inheritdoc />
        public NumericComparisonOptimizer(ILogger<NumericComparisonOptimizer> logger) : base(new NumericComparisonOptimizerImplementation(), logger)
        {

        }

        /// <summary>
        /// Implementation for <see cref="NumericComparisonOptimizer"/>
        /// </summary>
        public class NumericComparisonOptimizerImplementation
            : BaseRelationalOptimizerImplementation<object>
        {
            /// <inheritdoc />
            protected override IFilterCondition Transform(ComparisonCondition toTransform, OptimizationContext data)
            {
                var left = toTransform.LeftOperand;
                var right = toTransform.RightOperand;

                if (left is ColumnExpression leftColumnExpression && right is ConstantExpression rightConstantExpression)
                {
                    return Transform(toTransform, leftColumnExpression, rightConstantExpression, data.Context);
                }
                else if (right is ColumnExpression rightColumnExpression && left is ConstantExpression leftConstantExpression)
                {
                    return Transform(toTransform, rightColumnExpression, leftConstantExpression, data.Context);
                }
                else
                {
                    return toTransform;
                }
            }

            private IFilterCondition Transform(ComparisonCondition original, ColumnExpression columnExpression,
                ConstantExpression constantExpression, IQueryContext context)
            {
                if (columnExpression.SqlType.IsNumeric && constantExpression.SqlType.IsString && (original.ComparisonType == ComparisonTypes.EqualTo || original.ComparisonType == ComparisonTypes.NotEqualTo))
                {
                    if (columnExpression.SqlType.IsInt)
                    {
                        if (int.TryParse(constantExpression.Value.ToString(), out int iValue))
                        {
                            var newConstantExpression = new ConstantExpression(iValue, context);
                            return new ComparisonCondition(columnExpression, newConstantExpression, original.ComparisonType);
                        }
                        else
                        {
                            if(original.ComparisonType == ComparisonTypes.EqualTo)
                                return new AlwaysFalseCondition();
                            else
                                return new AlwaysTrueCondition();
                        }
                    }
                    // TODO: Add support for floats
                }

                return original;
            }
        }
    }
}
