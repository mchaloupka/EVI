using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.Query
{
    /// <summary>
    /// Variable representation
    /// </summary>
    public interface ICalculusVariable
    {
        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        DataType SqlType { get; }
    }
}
