using System;
using System.Collections.Generic;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping
{
    /// <summary>
    /// Cache for R2RML
    /// </summary>
    public class R2RMLCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="R2RMLCache"/> class.
        /// </summary>
        public R2RMLCache()
        {
            _sqlStatementCache = new CacheDictionary<ITriplesMap,string>(x => x.SqlQuery);
            _sqlTableCache = new CacheDictionary<ITriplesMap, string>(x => x.TableName);
        }

        /// <summary>
        /// The SQL statement cache
        /// </summary>
        private readonly CacheDictionary<ITriplesMap, string> _sqlStatementCache;

        /// <summary>
        /// Gets the SQL statement.
        /// </summary>
        /// <param name="triplesMap">The triples map.</param>
        /// <returns>SQL statement</returns>
        public string GetSqlStatement(ITriplesMap triplesMap)
        {
            return _sqlStatementCache.GetValueFor(triplesMap);
        }

        /// <summary>
        /// The SQL table cache
        /// </summary>
        private readonly CacheDictionary<ITriplesMap, string> _sqlTableCache;

        /// <summary>
        /// Gets the SQL table.
        /// </summary>
        /// <param name="triplesMap">The triples map.</param>
        /// <returns>SQL table name</returns>
        public string GetSqlTable(ITriplesMap triplesMap)
        {
            return _sqlTableCache.GetValueFor(triplesMap);
        }

        /// <summary>
        /// Cache dictionary
        /// </summary>
        /// <typeparam name="TK">Key type</typeparam>
        /// <typeparam name="T">Value type</typeparam>
        private class CacheDictionary<TK, T>
        {
            /// <summary>
            /// The get value function
            /// </summary>
            private readonly Func<TK, T> _getFunc;

            /// <summary>
            /// The cache
            /// </summary>
            private readonly Dictionary<TK, T> _cache;

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheDictionary{K, T}"/> class.
            /// </summary>
            /// <param name="getFunc">The get value from key function.</param>
            public CacheDictionary(Func<TK, T> getFunc)
            {
                _getFunc = getFunc;
                _cache = new Dictionary<TK, T>();
            }

            /// <summary>
            /// Gets the value for the key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns>The value.</returns>
            public T GetValueFor(TK key)
            {
                if (!_cache.ContainsKey(key))
                {
                    _cache.Add(key, _getFunc(key));
                }

                return _cache[key];
            }
        }
    }
}
