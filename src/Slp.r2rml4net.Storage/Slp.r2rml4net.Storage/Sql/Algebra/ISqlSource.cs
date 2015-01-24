using System.Collections.Generic;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    /// <summary>
    /// SQL source
    /// </summary>
    public interface ISqlSource: IVisitable<ISqlSourceVisitor>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        IEnumerable<ISqlColumn> Columns { get; }
    }

    /// <summary>
    /// SQL source that is in fact a statement
    /// </summary>
    public interface INotSqlOriginalDbSource : ISqlSource
    {
        /// <summary>
        /// Adds the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        void AddValueBinder(IBaseValueBinder valueBinder);

        /// <summary>
        /// Replaces the value binder.
        /// </summary>
        /// <param name="oldBinder">The old binder.</param>
        /// <param name="newBinder">The new binder.</param>
        void ReplaceValueBinder(IBaseValueBinder oldBinder, IBaseValueBinder newBinder);

        /// <summary>
        /// Removes the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        void RemoveValueBinder(IBaseValueBinder valueBinder);

        /// <summary>
        /// Gets the value binders.
        /// </summary>
        /// <value>The value binders.</value>
        IEnumerable<IBaseValueBinder> ValueBinders { get; }
    }

    /// <summary>
    /// SQL Source that is actually in the database
    /// </summary>
    public interface ISqlOriginalDbSource : ISqlSource
    {
        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>ISqlColumn.</returns>
        ISqlColumn GetColumn(string column);
    }
}
