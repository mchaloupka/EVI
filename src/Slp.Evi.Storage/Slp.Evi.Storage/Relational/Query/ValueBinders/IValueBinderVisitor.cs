using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Relational.Query.ValueBinders
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

        /// <summary>
        /// Visits <see cref="CoalesceValueBinder"/>
        /// </summary>
        /// <param name="coalesceValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(CoalesceValueBinder coalesceValueBinder, object data);

        /// <summary>
        /// Visits <see cref="SwitchValueBinder"/>
        /// </summary>
        /// <param name="switchValueBinder">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(SwitchValueBinder switchValueBinder, object data);
    }
}
