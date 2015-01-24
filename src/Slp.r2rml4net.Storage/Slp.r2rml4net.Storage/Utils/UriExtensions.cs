using System;

namespace Slp.r2rml4net.Storage.Utils
{
    /// <summary>
    /// Extensions for Uri
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Are the URIs equal.
        /// </summary>
        /// <param name="first">The first URI.</param>
        /// <param name="second">The second URI.</param>
        /// <returns><c>true</c> if the URIs are equal, <c>false</c> otherwise.</returns>
        public static bool UriEquals(this Uri first, Uri second)
        {
            return first.Equals(second) && string.Equals(first.Fragment, second.Fragment);
        }
    }
}
