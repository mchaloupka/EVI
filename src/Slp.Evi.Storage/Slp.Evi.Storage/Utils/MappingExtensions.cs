using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Query;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Utils
{
    /// <summary>
    /// Extensions for mapping classes.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Gets type resolver for passed <see cref="ITermMap"/>.
        /// </summary>
        public static Func<string, DataType> GetTypeResolver(this ITermMap termMap, IQueryContext context)
        {
            var triplesMap = termMap.TriplesMap;

            if (triplesMap == null)
            {
                // TODO: Remove this, replace by better solution, it happens only in tests
                return x => null;
            }
            else
            {
                var tableName = context.Mapping.Cache.GetSqlTable(triplesMap);

                if (tableName != null)
                {
                    var tableInfo = context.SchemaProvider.GetTableInfo(tableName);
                    return x => tableInfo.FindColumn(x).DataType;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
