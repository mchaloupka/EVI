using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Common.Database;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlColumn
        : ISqlColumnSchema
    {
        public MsSqlColumn(string name, MsSqlColumnType columnType)
        {
            Name = name;
            SqlType = columnType;
        }

        public static MsSqlColumn CreateFromDatabase(DatabaseColumn column)
        {
            return new MsSqlColumn(column.Name, new MsSqlColumnType(column.DbDataType, column.Nullable));
        }

        public string Name { get; }

        /// <inheritdoc />
        public ISqlColumnType SqlType { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Column({Name})";
        }
    }
}
