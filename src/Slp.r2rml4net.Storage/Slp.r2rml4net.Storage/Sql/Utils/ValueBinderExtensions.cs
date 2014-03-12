using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Utils
{
    public static class ValueBinderExtensions
    {
        public static IBaseValueBinder GetSelectValueBinder(this IBaseValueBinder binder, SqlSelectOp first, QueryContext context)
        {
            var newBinder = (IBaseValueBinder)binder.Clone();

            foreach (var col in newBinder.AssignedColumns.ToArray())
            {
                newBinder.ReplaceAssignedColumn(col, first.GetSelectColumn(col));
            }

            return newBinder;
        }

        public static IBaseValueBinder GetOriginalValueBinder(this IBaseValueBinder binder, QueryContext context)
        {
            var newBinder = (IBaseValueBinder)binder.Clone();

            foreach (var oldColumn in newBinder.AssignedColumns.ToArray())
            {
                if (!(oldColumn is SqlSelectColumn))
                {
                    throw new Exception("Can't get original value binder if it is not from sql select columns");
                }

                newBinder.ReplaceAssignedColumn(oldColumn, ((SqlSelectColumn)oldColumn).OriginalColumn);
            }

            return newBinder;
        }
    }
}
