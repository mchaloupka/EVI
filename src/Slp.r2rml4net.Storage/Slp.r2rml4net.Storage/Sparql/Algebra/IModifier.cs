using Slp.r2rml4net.Storage.Sparql.Algebra.Modifiers;
using Slp.r2rml4net.Storage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra
{
    /// <summary>
    /// Base interface for SPARQL result modifiers
    /// </summary>
    public interface IModifier
        : ISparqlQuery, IVisitable<IModifierVisitor>
    {
    }
}
