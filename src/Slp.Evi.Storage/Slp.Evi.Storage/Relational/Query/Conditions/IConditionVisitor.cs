using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;

namespace Slp.Evi.Storage.Relational.Query.Conditions
{
    /// <summary>
    /// Visitor for conditions
    /// </summary>
    public interface IConditionVisitor 
        : IFilterConditionVisitor, IAssignmentConditionVisitor, ISourceConditionVisitor
    {
        
    }
}
