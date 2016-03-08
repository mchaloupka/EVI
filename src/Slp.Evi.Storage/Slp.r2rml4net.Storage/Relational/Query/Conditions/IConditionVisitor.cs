using Slp.r2rml4net.Storage.Relational.Query.Conditions.Assignment;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
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
