using System;
using System.Collections.Generic;
using System.Data.Common;
using DatabaseSchemaReader;
using Slp.Evi.Common.Database;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlDatabaseSchema
        : ISqlDatabaseSchema
    {
        private readonly Dictionary<string, MsSqlTable> _tables;

        private MsSqlDatabaseSchema(Dictionary<string, MsSqlTable> tables)
        {
            _tables = tables;
        }

        public static MsSqlDatabaseSchema CreateFromDatabase(DbConnection connection)
        {
            var tables = new Dictionary<string, MsSqlTable>();

            using (var dbReader = new DatabaseReader(connection))
            {
                var schema = dbReader.ReadAll();

                foreach (var tableSchema in schema.Tables)
                {
                    tables.Add(tableSchema.Name, MsSqlTable.CreateFromDatabase(tableSchema));
                }
            }

            return new MsSqlDatabaseSchema(tables);
        }

        /// <inheritdoc />
        public ISqlTableSchema GetTable(string tableName)
        {
            if (_tables.TryGetValue(tableName, out var table))
            {
                return table;
            }
            else
            {
                throw new ArgumentException($"The table {tableName} does not exist", nameof(tableName));
            }
        }

        /// <inheritdoc />
        public ISqlColumnType GetCommonType(ISqlColumnType left, ISqlColumnType right)
        {
            if (!(left is MsSqlColumnType leftType && right is MsSqlColumnType rightType))
            {
                throw new InvalidOperationException($"Cannot decide the common type as one of the types is not MS SQL type: {left.GetType()}, {right.GetType()}");
            }
            else
            {
                return leftType.GetCommonType(rightType);
            }
        }

        /// <inheritdoc />
        public ISqlColumnType NullType => NullMsSqlColumnType.Instance;

        /// <inheritdoc />
        public ISqlColumnType IntegerType => new IntegerMsSqlColumnType(IntegerMsSqlColumnType.IntegerTypes.Int);

        /// <inheritdoc />
        public ISqlColumnType StringType => VarCharMsSqlColumnType.NVarCharMaxType;

        /// <inheritdoc />
        public ISqlColumnType DoubleType =>
            new FloatingPointMsSqlColumnType(FloatingPointMsSqlColumnType.FloatingTypes.Float);

        /// <inheritdoc />
        public ISqlColumnType BooleanType => new IntegerMsSqlColumnType(IntegerMsSqlColumnType.IntegerTypes.Bit);
    }
}
