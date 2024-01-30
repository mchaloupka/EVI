using Slp.Evi.Storage.Core.Database;

namespace Slp.Evi.Storage.MySql.Reader
{
    public class MySqlReaderColumn
        : ISqlResultColumn
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public VariableValue VariableValue { get; }

        public MySqlReaderColumn(string name, VariableValue value)
        {
            Name = name;
            VariableValue = value;
        }
    }
}