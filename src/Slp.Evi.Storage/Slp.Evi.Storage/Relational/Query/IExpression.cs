using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Relational.Query
{
    /// <summary>
    /// The relational expression
    /// </summary>
    public interface IExpression
        : IVisitable<IExpressionVisitor>
    {
        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        DataType SqlType { get; }

        /// <summary>
        /// Gets the used calculus variables.
        /// </summary>
        /// <value>The used calculus variables.</value>
        IEnumerable<ICalculusVariable> UsedCalculusVariables { get; }
    }
}
