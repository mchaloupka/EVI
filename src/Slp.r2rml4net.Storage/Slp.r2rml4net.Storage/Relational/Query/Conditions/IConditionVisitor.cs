using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions
{
    /// <summary>
    /// Visitor for conditions
    /// </summary>
    public interface IConditionVisitor 
        : IFilterConditionVisitor, IAssignmentConditionVisitor, ISourceConditionVisitor
    {
        
    }
}
