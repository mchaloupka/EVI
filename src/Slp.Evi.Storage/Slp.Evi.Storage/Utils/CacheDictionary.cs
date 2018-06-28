using System;
using System.Collections.Concurrent;

namespace Slp.Evi.Storage.Utils
{
    /// <summary>
    /// Cache dictionary
    /// </summary>
    /// <typeparam name="TK">Key type</typeparam>
    /// <typeparam name="T">Value type</typeparam>
    public class CacheDictionary<TK, T>
    {
        /// <summary>
        /// The get value function
        /// </summary>
        private readonly Func<TK, T> _getFunc;

        /// <summary>
        /// The cache
        /// </summary>
        private readonly ConcurrentDictionary<TK, T> _cache = new ConcurrentDictionary<TK, T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheDictionary{K, T}"/> class.
        /// </summary>
        /// <param name="getFunc">The get value from key function.</param>
        public CacheDictionary(Func<TK, T> getFunc)
        {
            _getFunc = getFunc;
        }

        /// <summary>
        /// Gets the value for the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public T GetValueFor(TK key)
        {
            return _cache.GetOrAdd(key, _getFunc);
        }
    }
}