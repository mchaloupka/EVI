using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Relational.Query.Expression;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query
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
    }
}
