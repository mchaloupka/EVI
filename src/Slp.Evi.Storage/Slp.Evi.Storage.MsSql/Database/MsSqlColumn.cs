using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Common.Database;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlColumn
        : ISqlColumnSchema
    {
        public MsSqlColumn(string name, MsSqlColumnType columnType, bool isNullable)
        {
            Name = name;
            SqlType = columnType;
            IsNullable = isNullable;
        }

        public static MsSqlColumn CreateFromDatabase(DatabaseColumn column)
        {
            return new MsSqlColumn(column.Name, MsSqlColumnType.Create(column.DbDataType), column.Nullable);
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool IsNullable { get; }

        /// <inheritdoc />
        public ISqlColumnType SqlType { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Column({Name})";
        }
    }
}
