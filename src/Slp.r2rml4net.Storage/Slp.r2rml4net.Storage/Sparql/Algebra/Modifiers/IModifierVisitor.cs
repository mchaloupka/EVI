using Slp.r2rml4net.Storage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Modifiers
{
    /// <summary>
    /// Visitor interface of SPARQL result modifiers
    /// </summary>
    public interface IModifierVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="SelectModifier"/>
        /// </summary>
        /// <param name="selectModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(SelectModifier selectModifier, object data);
    }
}
