using System.Collections.Generic;

namespace Slp.Evi.Storage.Relational.Query
{
    /// <summary>
    /// Relational query
    /// </summary>
    public class RelationalQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalQuery"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="valueBinders">The value binders.</param>
        public RelationalQuery(ICalculusSource model, IEnumerable<IValueBinder> valueBinders)
        {
            Model = model;
            ValueBinders = valueBinders;
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public ICalculusSource Model { get; }

        /// <summary>
        /// Gets the value binders.
        /// </summary>
        /// <value>The value binders.</value>
        public IEnumerable<IValueBinder> ValueBinders { get; }
    }
}
