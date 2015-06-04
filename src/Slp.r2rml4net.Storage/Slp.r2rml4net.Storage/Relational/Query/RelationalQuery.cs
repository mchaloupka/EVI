using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query
{
    public class RelationalQuery
    {
        public RelationalQuery(ICalculusSource model, IEnumerable<IValueBinder> valueBinders)
        {
            this.Model = model;
            this.ValueBinders = valueBinders;
        }

        public ICalculusSource Model { get; private set; }

        public IEnumerable<IValueBinder> ValueBinders { get; private set; }
    }
}
