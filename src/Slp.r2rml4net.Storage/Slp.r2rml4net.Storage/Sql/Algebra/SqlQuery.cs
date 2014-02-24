using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    public class SqlQuery : ISqlQuery
    {
        public SqlQuery(ISqlSource source)
        {
            this.SqlSource = source;
            this.valueBinders = new List<ValueBinder>();
        }

        public ISqlSource SqlSource { get; private set; }

        private List<ValueBinder> valueBinders;

        public void AddValueBinder(ValueBinder valueBinder)
        {
            this.valueBinders.Add(valueBinder);
        }
    }
}
