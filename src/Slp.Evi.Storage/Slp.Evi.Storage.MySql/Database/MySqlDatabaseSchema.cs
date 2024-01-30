using System;
using System.Collections.Generic;
using System.Data.Common;
using DatabaseSchemaReader;
using Slp.Evi.Storage.Core.Common.Database;

namespace Slp.Evi.Storage.MySql.Database
{
    public class MySqlDatabaseSchema
        : ISqlDatabaseSchema
    {
        private readonly Dictionary<string, MySqlTable> _tables;

        private MySqlDatabaseSchema(Dictionary<string, MySqlTable> tables)
        {
            _tables = tables;
        }

        public static MySqlDatabaseSchema CreateFromDatabase(DbConnection connection)
        {
            var tables = new Dictionary<string, MySqlTable>();

            using (var dbReader = new DatabaseReader(connection))
            {
                var schema = dbReader.ReadAll();

                foreach (var tableSchema in schema.Tables)
                {
                    tables.Add(tableSchema.Name, MySqlTable.CreateFromDatabase(tableSchema));
                }
            }

            return new MySqlDatabaseSchema(tables);
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
            if (!(left is MySqlColumnType leftType && right is MySqlColumnType rightType))
            {
                throw new InvalidOperationException($"Cannot decide the common type as one of the types is not MS SQL type: {left.GetType()}, {right.GetType()}");
            }
            else
            {
                return leftType.GetCommonType(rightType);
            }
        }

        /// <inheritdoc />
        public ISqlColumnType NullType => NullMySqlColumnType.Instance;

        /// <inheritdoc />
        public ISqlColumnType IntegerType => new IntegerMySqlColumnType(IntegerMySqlColumnType.IntegerTypes.Int);

        /// <inheritdoc />
        public ISqlColumnType StringType => VarCharMySqlColumnType.VarCharMaxType;

        /// <inheritdoc />
        public ISqlColumnType DoubleType =>
            new FloatingPointMySqlColumnType(FloatingPointMySqlColumnType.FloatingTypes.Float);

        /// <inheritdoc />
        public ISqlColumnType BooleanType => new IntegerMySqlColumnType(IntegerMySqlColumnType.IntegerTypes.Bit);

        /// <inheritdoc />
        public ISqlColumnType DateTimeType => new DateTimeMySqlColumnType(DateTimeMySqlColumnType.DateTimeTypes.DateTime);
    }
}
