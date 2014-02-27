using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    public class NoRowSource : ISqlSource
    {
        public string Name { get; set; }


        public IEnumerable<ISqlColumn> Columns
        {
            get { yield break; }
        }
    }
}
