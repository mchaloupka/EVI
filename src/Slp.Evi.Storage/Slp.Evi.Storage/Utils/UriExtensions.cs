using System;

namespace Slp.Evi.Storage.Utils
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

        /// <summary>
        /// Determines whether two URIs are equal (including fragment)
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="other">The other URI.</param>
        public static bool IsCompleteUriEqualTo(this Uri uri, Uri other)
        {
            if (uri == null)
            {
                return other == null;
            }
            else if (other == null)
            {
                return false;
            }
            else
            {
                return (uri == other) && (uri.Fragment == other.Fragment);
            }
        }
    }
}
