using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Query;

namespace Slp.Evi.Storage.Utils
{
    /// <summary>
    /// Extensions for mapping classes.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Gets type resolver for passed <see cref="ITermMapping"/>.
        /// </summary>
        public static Func<string, DataType> GetTypeResolver(this ITermMapping termMap, IQueryContext context)
        {
            var triplesMap = termMap.TriplesMap;

            if (triplesMap == null)
            {
                // TODO: Remove this, replace by better solution, it happens only in tests
                return x => null;
            }
            else
            {
                var tableName = triplesMap.TableName;

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
