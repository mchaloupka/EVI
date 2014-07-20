using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    /// <summary>
    /// Extension that provide additional operator info
    /// </summary>
    public static class OperatorInfoExtension
    {
        /// <summary>
        /// Determines whether the join operator can be merged with another one.
        /// </summary>
        /// <param name="joinOp">The join op.</param>
        /// <returns><c>true</c> if the join operator can be merged with another one; otherwise, <c>false</c>.</returns>
        public static bool CanBeMergedTo(this SqlSelectOp joinOp)
        {
            if (joinOp.Offset.HasValue || joinOp.Limit.HasValue || joinOp.Orderings.Any() || joinOp.IsDistinct || joinOp.IsReduced)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Determines whether the join operator can be merged with another one.
        /// </summary>
        /// <param name="second">The operator that will be merged to.</param>
        /// <param name="first">The operator that should merge to the other one.</param>
        /// <returns><c>true</c> if the join operator can be merged with another one.; otherwise, <c>false</c>.</returns>
        public static bool IsMergeableTo(this SqlSelectOp second, SqlSelectOp first)
        {
            if (!second.CanBeMergedTo())
                return false;

            foreach (var secondValBinder in second.ValueBinders)
            {
                var firstValBinder = first.ValueBinders.Where(x => x.VariableName == secondValBinder.VariableName).FirstOrDefault();

                if (firstValBinder == null)
                    continue;

                var neededColumns = secondValBinder.AssignedColumns.OfType<SqlSelectColumn>().Select(x => x.OriginalColumn);
                var neededColumnsNotInOriginalSource = neededColumns.Where(x => x.Source != second.OriginalSource);

                if (neededColumnsNotInOriginalSource.Any())
                    return false;
            }

            return true;
        }
    }
}
