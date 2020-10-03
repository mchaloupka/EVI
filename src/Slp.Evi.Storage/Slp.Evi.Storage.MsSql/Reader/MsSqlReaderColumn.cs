using Slp.Evi.Database;

namespace Slp.Evi.Storage.MsSql.Reader
{
    public class MsSqlReaderColumn
        : ISqlResultColumn
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public VariableValue VariableValue { get; }

        public MsSqlReaderColumn(string name, VariableValue value)
        {
            Name = name;
            VariableValue = value;
        }
    }
}