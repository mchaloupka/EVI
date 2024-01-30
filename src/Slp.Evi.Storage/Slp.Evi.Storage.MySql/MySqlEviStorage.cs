using System;
using Slp.Evi.Storage.MySql.Database;
using TCode.r2rml4net;

namespace Slp.Evi.Storage.MySql
{
    public sealed class MySqlEviStorage
        : EviStorage<MySqlQuery>
    {
        public MySqlEviStorage(IR2RML mapping, string connectionString, int queryTimeout)
            : base(mapping, new MySqlDatabase(connectionString, queryTimeout))
        { }

        public MySqlEviStorage(IR2RML mapping, MySqlDatabase database)
            : base(mapping, database)
        { }
    }
}
