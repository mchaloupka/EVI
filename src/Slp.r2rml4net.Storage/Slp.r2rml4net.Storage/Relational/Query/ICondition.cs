using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Assignment;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query
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
