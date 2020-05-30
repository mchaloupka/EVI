using Slp.Evi.Storage.Common;
using TCode.r2rml4net;

namespace Slp.Evi.Storage.MsSql
{
    public sealed class MsSqlEviStorage
        : EviStorage
    {
        /// <inheritdoc />
        public MsSqlEviStorage(IR2RML mapping) 
            : base(mapping)
        {

        }
    }
}
