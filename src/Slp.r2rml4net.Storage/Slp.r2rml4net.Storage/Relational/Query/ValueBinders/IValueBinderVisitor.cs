using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// Visitor for value binders
    /// </summary>
    public interface IValueBinderVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="BaseValueBinder"/>
        /// </summary>
        /// <param name="baseValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(BaseValueBinder baseValueBinder, object data);

        /// <summary>
        /// Visits <see cref="EmptyValueBinder"/>
        /// </summary>
        /// <param name="emptyValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(EmptyValueBinder emptyValueBinder, object data);
    }
}
