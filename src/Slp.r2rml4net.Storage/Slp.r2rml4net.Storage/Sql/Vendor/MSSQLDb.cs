using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    public class MSSQLDb : BaseSqlDb
    {
        private string connectionString;

        public MSSQLDb(string connectionString)
        {
            this.connectionString = connectionString;
        }
    }
}
