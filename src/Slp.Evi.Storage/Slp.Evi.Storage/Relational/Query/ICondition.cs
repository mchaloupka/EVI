using System.Collections.Generic;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Relational.Query
{
    /// <summary>
    /// Condition representation
    /// </summary>
    public interface ICondition
    {

    }

    /// <summary>
    /// Source condition
    /// </summary>
    public interface ISourceCondition
        : ICondition, IVisitable<ISourceConditionVisitor>
    {
        /// <summary>
        /// Gets the calculus variables.
        /// </summary>
        /// <value>The calculus variables.</value>
        IEnumerable<ICalculusVariable> CalculusVariables { get; }
    }

    /// <summary>
    /// Assignment condition
    /// </summary>
    public interface IAssignmentCondition
        : ICondition, IVisitable<IAssignmentConditionVisitor>
    {

    }

    /// <summary>
    /// Filter condition
    /// </summary>
    public interface IFilterCondition
        : ICondition, IVisitable<IFilterConditionVisitor>
    {
        
    }
}
