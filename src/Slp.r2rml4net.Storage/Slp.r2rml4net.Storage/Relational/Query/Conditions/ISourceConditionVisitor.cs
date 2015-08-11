using Slp.r2rml4net.Storage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions
{
    /// <summary>
    /// Visitor for source conditions
    /// </summary>
    public interface ISourceConditionVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="tupleFromSourceCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data);
    }
}
