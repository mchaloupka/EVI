using System;
using Slp.r2rml4net.Storage.Database.Base;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Condition;
using Slp.r2rml4net.Storage.Relational.Query.Source;

namespace Slp.r2rml4net.Storage.Query
{
    /// <summary>
    /// The query naming helpers.
    /// </summary>
    public class QueryNamingHelpers
    {
        private readonly QueryContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryNamingHelpers"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public QueryNamingHelpers(QueryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the source of variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="currentModel">The current model.</param>
        public ICondition GetSourceOfVariable(ICalculusVariable variable, CalculusModel currentModel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the name of the source condition.
        /// </summary>
        /// <param name="sourceCondition">The source condition.</param>
        public string GetSourceConditionName(ISourceCondition sourceCondition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <param name="sourceCondition">The source condition.</param>
        /// <param name="variable">The variable.</param>
        public string GetVariableName(ISourceCondition sourceCondition, ICalculusVariable variable)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the source condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="sourceCondition">The source condition.</param>
        public void AddSourceCondition(CalculusModel condition, ISourceCondition sourceCondition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the assignment condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="assignmentCondition">The assignment condition.</param>
        public void AddAssignmentCondition(CalculusModel condition, IAssignmentCondition assignmentCondition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the tuple from source condtion.
        /// </summary>
        /// <param name="parentModel">The parent model.</param>
        /// <param name="baseSqlQueryBuilder">The base SQL query builder.</param>
        public TupleFromSourceCondition GetTupleFromSourceCondtion(CalculusModel parentModel, BaseSqlQueryBuilder baseSqlQueryBuilder)
        {
            throw new NotImplementedException();
        }
    }
}