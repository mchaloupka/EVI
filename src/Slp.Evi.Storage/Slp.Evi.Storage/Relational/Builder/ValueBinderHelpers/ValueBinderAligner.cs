using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Sources;

namespace Slp.Evi.Storage.Relational.Builder.ValueBinderHelpers
{
    public class ValueBinderAligner
    {
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
            throw new NotImplementedException();
        }
    }
}
