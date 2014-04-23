using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Utils
{
    public static class UriExtensions
    {
        public static bool UriEquals(this Uri first, Uri second)
        {
            return first.Equals(second) && string.Equals(first.Fragment, second.Fragment);
        }
    }
}
