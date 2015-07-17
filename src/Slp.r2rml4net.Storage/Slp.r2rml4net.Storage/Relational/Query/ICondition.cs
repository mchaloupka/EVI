using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query.Condition;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query
{
    /// <summary>
    /// Condition representation
    /// </summary>
    public interface ICondition : IVisitable<IConditionVisitor>
    {

    }

    /// <summary>
    /// Source condition
    /// </summary>
    public interface ISourceCondition
        : ICondition
    {

    }

    /// <summary>
    /// Assignment condition
    /// </summary>
    public interface IAssignmentCondition
        : ICondition
    {

    }

    /// <summary>
    /// Filter condition
    /// </summary>
    public interface IFilterCondition
        : ICondition
    {
        
    }
}
