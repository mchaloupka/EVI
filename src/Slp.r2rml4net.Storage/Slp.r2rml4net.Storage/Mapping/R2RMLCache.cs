using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Mapping
{
    public class R2RMLCache
    {
        private MappingProcessor mappingProcessor;

        public R2RMLCache(MappingProcessor mappingProcessor)
        {
            this.mappingProcessor = mappingProcessor;

            this.sqlStatementCache = new CacheDictionary<ITriplesMap,string>(x => x.SqlQuery);
            this.sqlTableCache = new CacheDictionary<ITriplesMap, string>(x => x.TableName);
        }

        private CacheDictionary<ITriplesMap, string> sqlStatementCache;
        public string GetSqlStatement(ITriplesMap triplesMap)
        {
            return this.sqlStatementCache.GetValueFor(triplesMap);
        }

        private CacheDictionary<ITriplesMap, string> sqlTableCache;
        public string GetSqlTable(ITriplesMap triplesMap)
        {
            return this.sqlTableCache.GetValueFor(triplesMap);
        }

        private class CacheDictionary<K, T>
        {
            private Func<K, T> getFunc;
            private Dictionary<K, T> cache;

            public CacheDictionary(Func<K, T> getFunc)
            {
                this.getFunc = getFunc;
                this.cache = new Dictionary<K, T>();
            }

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
