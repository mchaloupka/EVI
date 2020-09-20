using System;
using Slp.Evi.Database;

namespace Slp.Evi.Storage.MsSql.Reader
{
    public class MsSqlReaderColumn
        : ISqlResultColumn
    {
        private readonly object _value;

        public string Name { get; }

        public MsSqlReaderColumn(string name, object value)
        {
            Name = name;
            _value = value is DBNull ? null : value;
        }
    }
}