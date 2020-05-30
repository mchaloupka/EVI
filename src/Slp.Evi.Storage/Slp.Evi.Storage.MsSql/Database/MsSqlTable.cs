using System;
using System.Collections.Generic;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlTable
    {
        public MsSqlTable(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static MsSqlTable CreateFromDatabase(DatabaseTable tableSchema)
        {
            return new MsSqlTable(tableSchema.Name);
        }
    }
}
