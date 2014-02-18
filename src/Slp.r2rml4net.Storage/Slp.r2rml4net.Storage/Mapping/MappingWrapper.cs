using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Mapping
{
    public class MappingWrapper
    {
        private IR2RML mapping;

        public MappingWrapper(IR2RML mapping)
        {
            this.mapping = mapping;
        }
    }
}
