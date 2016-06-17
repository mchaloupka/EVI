using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// Parameter passed to <see cref="ISparqlExpressionVisitor"/> visit methods.
    /// </summary>
    public class ExpressionVisitParameter
    {
        /// <summary>
        /// Gets the query context.
        /// </summary>
        public QueryContext QueryContext { get; private set; }

        /// <summary>
        /// Gets the available value binders.
        /// </summary>
        public Dictionary<string, IValueBinder> ValueBinders { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionVisitParameter"/> class.
        /// </summary>
        /// <param name="queryContext">The query context.</param>
        /// <param name="valueBinders">The value binders.</param>
        public ExpressionVisitParameter(QueryContext queryContext, IEnumerable<IValueBinder> valueBinders)
        {
            QueryContext = queryContext;
            ValueBinders = valueBinders.ToDictionary(x => x.VariableName);
        }
    }
}