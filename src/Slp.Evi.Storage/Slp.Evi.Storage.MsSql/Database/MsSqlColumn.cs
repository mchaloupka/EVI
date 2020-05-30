using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlColumn
    {
        private MsSqlColumnType _columnType;

        public MsSqlColumn(string name, MsSqlColumnType columnType)
        {
            Name = name;
            _columnType = columnType;
        }

        public static MsSqlColumn CreateFromDatabase(DatabaseColumn column)
        {
            return new MsSqlColumn(column.Name, new MsSqlColumnType(column.DbDataType));
        }

        public string Name { get; }

        public LiteralValueType DefaultRdfType => _columnType.DefaultRdfType;
    }
}
