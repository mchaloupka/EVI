using System;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    /// <summary>
    /// SQL Expression
    /// </summary>
    public interface IExpression : ICloneable, IVisitable<IExpressionVisitor>
    {
        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        DataType SqlType { get; }
    }
}
