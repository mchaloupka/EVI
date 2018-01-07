using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Query.ValueBinders;

namespace Slp.Evi.Storage.Relational.Builder.ValueBinderHelpers
{
    /// <summary>
    /// The aligner for <see cref="IValueBinder"/>.
    /// </summary>
    public class ValueBinderAligner
    {
        private readonly ValueBinderFlattener _valueBinderFlattener;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBinderAligner"/> class.
        /// </summary>
        /// <param name="conditionBuilder">The condition builder.</param>
        public ValueBinderAligner(ConditionBuilder conditionBuilder)
        {
            this._valueBinderFlattener = new ValueBinderFlattener(conditionBuilder);
        }

        /// <summary>
        /// Aligns the specified query.
        /// </summary>
        /// <param name="toAlign">Query to align.</param>
        /// <param name="queryContext">The query context.</param>
        public RelationalQuery Align(RelationalQuery toAlign, IQueryContext queryContext)
        {
            var valueBinders = toAlign.ValueBinders.ToList();
            var model = toAlign.Model;

            if (model is ModifiedCalculusModel modifiedCalculusModel)
            {
                var calculusModel = modifiedCalculusModel.InnerModel;
                var newModel = Align(calculusModel, valueBinders, queryContext);

                if (newModel.changed)
                {
                    var modifiedModel = new ModifiedCalculusModel(newModel.model, modifiedCalculusModel.Ordering,
                        modifiedCalculusModel.Limit, modifiedCalculusModel.Offset, modifiedCalculusModel.IsDistinct);

                    return new RelationalQuery(modifiedModel, newModel.valueBinders);
                }
                else
                {
                    return toAlign;
                }

            }
            else if (model is CalculusModel calculusModel)
            {
                 var newModel = Align(calculusModel, valueBinders, queryContext);

                if (newModel.changed)
                {
                    return new RelationalQuery(newModel.model, newModel.valueBinders);
                }
                else
                {
                    return toAlign;
                }
            }
            else
            {
                throw new Exception($"Expected {nameof(ModifiedCalculusModel)} or {nameof(CalculusModel)}");
            }
        }

        private (CalculusModel model, IEnumerable<IValueBinder> valueBinders, bool changed) Align(CalculusModel calculusModel, IEnumerable<IValueBinder> valueBinders, IQueryContext queryContext)
        {
            List<IValueBinder> resultingBinders = new List<IValueBinder>();
            List<IAssignmentCondition> assignmentConditions = new List<IAssignmentCondition>();
            bool changed = false;
            var originalBinders = valueBinders.ToList();

            foreach (var valueBinder in originalBinders)
            {
                var flattened = _valueBinderFlattener.Flatten(valueBinder, queryContext).ToList();

                if (flattened.Select(x => x.valueBinder).Any(x => !(x is BaseValueBinder)))
                {
                    resultingBinders.Add(ConvertToExpressionSetValueBinder(valueBinder, queryContext));
                    changed = true;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            // If some assignment needed then modify model
            // else: ...
            if (assignmentConditions.Count > 0)
            {
                var conditions = new List<ICondition>();
                conditions.AddRange(assignmentConditions);
                conditions.AddRange(calculusModel.AssignmentConditions);
                conditions.AddRange(calculusModel.FilterConditions);
                conditions.AddRange(calculusModel.SourceConditions);

                var variables = assignmentConditions.Select(x => x.Variable).ToList();
                variables.AddRange(calculusModel.Variables);

                var newModel = new CalculusModel(variables.Distinct().ToArray(), conditions);
                return (newModel, resultingBinders, true);
            }
            else if (changed)
            {
                return (calculusModel, resultingBinders, true);
            }
            else
            {
                return (calculusModel, originalBinders, false);
            }
        }

        private IValueBinder ConvertToExpressionSetValueBinder(IValueBinder valueBinder, IQueryContext queryContext)
        {
            throw new NotImplementedException();
        }
    }
}
