using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Relational.Query.Sources
{
    /// <summary>
    /// Model representing calculus representation of a query
    /// </summary>
    public class CalculusModel
        : ICalculusSource
    {
        /// <summary>
        /// The source conditions
        /// </summary>
        private readonly List<ISourceCondition> _sourceConditions;

        /// <summary>
        /// The filter conditions
        /// </summary>
        private readonly List<IFilterCondition> _filterConditions;

        /// <summary>
        /// The assignment conditions
        /// </summary>
        private readonly List<IAssignmentCondition> _assignmentConditions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculusModel"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="conditions">The conditions.</param>
        public CalculusModel(IEnumerable<ICalculusVariable> variables, IEnumerable<ICondition> conditions)
        {
            _sourceConditions = new List<ISourceCondition>();
            _filterConditions = new List<IFilterCondition>();
            _assignmentConditions = new List<IAssignmentCondition>();

            Variables = variables;

            foreach (var cond in conditions)
            {
                if (cond is ISourceCondition)
                {
                    _sourceConditions.Add((ISourceCondition) cond);
                }
                else if (cond is IFilterCondition)
                {
                    _filterConditions.Add((IFilterCondition) cond);
                }
                else if (cond is IAssignmentCondition)
                {
                    _assignmentConditions.Add((IAssignmentCondition) cond);
                }
                else
                {
                    throw new ArgumentException("Condition of unknown type found", nameof(conditions));
                }
            }
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<ICalculusVariable> Variables { get; private set; }

        /// <summary>
        /// Gets the source conditions.
        /// </summary>
        /// <value>The source conditions.</value>
        public IEnumerable<ISourceCondition> SourceConditions
        {
            get { return _sourceConditions; }
        }

        /// <summary>
        /// Gets the filter conditions.
        /// </summary>
        /// <value>The filter conditions.</value>
        public IEnumerable<IFilterCondition> FilterConditions
        {
            get { return _filterConditions; }
        }

        /// <summary>
        /// Gets the assignment conditions.
        /// </summary>
        /// <value>The assignment conditions.</value>
        public IEnumerable<IAssignmentCondition> AssignmentConditions
        {
            get { return _assignmentConditions; }
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ICalculusSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
