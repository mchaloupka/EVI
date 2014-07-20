using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    /// <summary>
    /// Interface IValueBinderVisitor
    /// </summary>
    public interface IValueBinderVisitor : IVisitor
    {
        /// <summary>
        /// Visits the specified case value binder.
        /// </summary>
        /// <param name="caseValueBinder">The case value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(CaseValueBinder caseValueBinder, object data);

        /// <summary>
        /// Visits the specified collate value binder.
        /// </summary>
        /// <param name="coalesceValueBinder">The coalesce value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(CoalesceValueBinder coalesceValueBinder, object data);

        /// <summary>
        /// Visits the specified value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(ValueBinder valueBinder, object data);

        /// <summary>
        /// Visits the specified SQL side value binder.
        /// </summary>
        /// <param name="sqlSideValueBinder">The SQL side value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(SqlSideValueBinder sqlSideValueBinder, object data);

        /// <summary>
        /// Visits the specified blank value binder.
        /// </summary>
        /// <param name="blankValueBinder">The blank value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(BlankValueBinder blankValueBinder, object data);

        /// <summary>
        /// Visits the specified expression value binder.
        /// </summary>
        /// <param name="expressionValueBinder">The expression value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(ExpressionValueBinder expressionValueBinder, object data);
    }
}
