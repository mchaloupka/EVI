using Slp.Evi.Storage.Query;

namespace Slp.Evi.Storage.Sparql.PostProcess.SafeAlgebra
{
    /// <summary>
    /// Parameter used in safe algebra optimizations
    /// </summary>
    public class SafeAlgebraParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeAlgebraParameter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="isNestedInLeftJoin">Indicates whether it is inside of left join.</param>
        public SafeAlgebraParameter(QueryContext context, bool isNestedInLeftJoin)
        {
            Context = context;
            IsNestedInLeftJoin = isNestedInLeftJoin;
        }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        public QueryContext Context { get; set; }

        /// <summary>
        /// Indicates whether it is inside of left join.
        /// </summary>
        public bool IsNestedInLeftJoin { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SafeAlgebraParameter"/> class as a clone but with modified <see cref="IsNestedInLeftJoin"/>.
        /// </summary>
        /// <param name="isNestedInLeftJoin">New value for <see cref="IsNestedInLeftJoin"/></param>
        public SafeAlgebraParameter Create(bool isNestedInLeftJoin)
        {
            return new SafeAlgebraParameter(Context, isNestedInLeftJoin);
        }
    }
}