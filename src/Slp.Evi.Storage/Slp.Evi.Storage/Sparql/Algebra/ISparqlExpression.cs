using System.Collections.Generic;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Sparql.Algebra
{
    /// <summary>
    /// Public interface for all sparql expressions
    /// </summary>
    public interface ISparqlExpression
        : IVisitable<ISparqlExpressionVisitor>
    {
        IEnumerable<string> NeededVariables { get; }
    }
}
