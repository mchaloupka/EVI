using Slp.Evi.Storage.Utils;
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
    }
}
