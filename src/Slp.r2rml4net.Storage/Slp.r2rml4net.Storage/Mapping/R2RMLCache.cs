using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Mapping
{
    /// <summary>
    /// Cache for R2RML
    /// </summary>
    public class R2RMLCache
    {
        /// <summary>
        /// The mapping processor
        /// </summary>
        private MappingProcessor mappingProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="R2RMLCache"/> class.
        /// </summary>
        /// <param name="mappingProcessor">The mapping processor.</param>
        public R2RMLCache(MappingProcessor mappingProcessor)
        {
            this.mappingProcessor = mappingProcessor;

            this.sqlStatementCache = new CacheDictionary<ITriplesMap,string>(x => x.SqlQuery);
            this.sqlTableCache = new CacheDictionary<ITriplesMap, string>(x => x.TableName);
        }

        /// <summary>
        /// The SQL statement cache
        /// </summary>
        private CacheDictionary<ITriplesMap, string> sqlStatementCache;

        /// <summary>
        /// Gets the SQL statement.
        /// </summary>
        /// <param name="triplesMap">The triples map.</param>
        /// <returns>SQL statement</returns>
        public string GetSqlStatement(ITriplesMap triplesMap)
        {
            return this.sqlStatementCache.GetValueFor(triplesMap);
        }

        /// <summary>
        /// The SQL table cache
        /// </summary>
        private CacheDictionary<ITriplesMap, string> sqlTableCache;

        /// <summary>
        /// Gets the SQL table.
        /// </summary>
        /// <param name="triplesMap">The triples map.</param>
        /// <returns>SQL table name</returns>
        public string GetSqlTable(ITriplesMap triplesMap)
        {
            return this.sqlTableCache.GetValueFor(triplesMap);
        }

        /// <summary>
        /// Cache dictionary
        /// </summary>
        /// <typeparam name="K">Key type</typeparam>
        /// <typeparam name="T">Value type</typeparam>
        private class CacheDictionary<K, T>
        {
            /// <summary>
            /// The get value function
            /// </summary>
            private Func<K, T> getFunc;

            /// <summary>
            /// The cache
            /// </summary>
            private Dictionary<K, T> cache;

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheDictionary{K, T}"/> class.
            /// </summary>
            /// <param name="getFunc">The get value from key function.</param>
            public CacheDictionary(Func<K, T> getFunc)
            {
                this.getFunc = getFunc;
                this.cache = new Dictionary<K, T>();
            }

            /// <summary>
            /// Gets the value for the key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns>The value.</returns>
            public T GetValueFor(K key)
            {
                if (!this.cache.ContainsKey(key))
                {
                    this.cache.Add(key, this.getFunc(key));
                }

                return this.cache[key];
            }
        }
    }
}
