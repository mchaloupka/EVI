using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Utils
{
    public static class OperatorInfoExtension
    {
        public static bool IsMergeableTo(this SqlSelectOp second, SqlSelectOp first)
        {
            if (first.Offset.HasValue || first.Limit.HasValue || first.Orderings.Any() || first.IsDistinct || first.IsReduced)
                return false;

            foreach (var secondValBinder in second.ValueBinders)
            {
                var firstValBinder = first.ValueBinders.Where(x => x.VariableName == secondValBinder.VariableName).FirstOrDefault();

                if (firstValBinder == null)
                    continue;

                var neededColumns = secondValBinder.AssignedColumns;
                var neededColumnsNotInOriginalSource = neededColumns.Where(x => x.Source != second.OriginalSource);

                if (neededColumnsNotInOriginalSource.Any())
                    return false;
            }

            return true;

            //List<ICondition> conditions = new List<ICondition>();

            //foreach (var firstValBinder in first.ValueBinders)
            //{
            //    foreach (var secondValBinder in second.ValueBinders)
            //    {
            //        if (firstValBinder.VariableName == secondValBinder.VariableName)
            //        {
            //            conditions.Add(conditionBuilder.CreateJoinEqualsCondition(context, firstValBinder.GetOriginalValueBinder(context), secondValBinder.GetOriginalValueBinder(context)));
            //        }
            //    }
            //}

            //ICondition condition = conditionBuilder.CreateAlwaysTrueCondition(context);

            //if (conditions.Count == 1)
            //{
            //    condition = conditions[0];
            //}
            //else if (conditions.Count > 1)
            //{
            //    condition = conditionBuilder.CreateAndCondition(context, conditions);
            //}
        }
    }
}
