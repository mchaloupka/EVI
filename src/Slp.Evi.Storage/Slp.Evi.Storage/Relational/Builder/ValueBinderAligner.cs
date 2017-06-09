using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Relational.Builder
{
    /// <summary>
    /// Aligner for <see cref="IValueBinder"/>.
    /// </summary>
    public class ValueBinderAligner
    {
        /// <summary>
        /// Aligns the <see cref="IValueBinder"/> in the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="queryContext">The query context.</param>
        public RelationalQuery Align(RelationalQuery query, IQueryContext queryContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
