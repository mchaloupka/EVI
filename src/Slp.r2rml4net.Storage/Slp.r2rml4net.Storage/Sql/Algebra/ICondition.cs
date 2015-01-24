using System;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    /// <summary>
    /// SQL Condition
    /// </summary>
    public interface ICondition : ICloneable, IVisitable<IConditionVisitor>
    {

    }
}
