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
    public static class MappingExtensions
    {
        /// <summary>
        /// Gets type resolver for passed <see cref="ITermMap"/>.
        /// </summary>
        public static Func<string, DataType> GetTypeResolver(this ITermMap termMap, IQueryContext context)
        {
            var triplesMap = termMap.GetTriplesMapConfiguration();

            if (triplesMap == null)
            {
                // TODO: Remove this, replace by better solution, it happens only in tests
                return x => null;
            }
            else if (triplesMap.TableName != null)
            {
                var tableInfo = context.SchemaProvider.GetTableInfo(triplesMap.TableName);
                return x => tableInfo.FindColumn(x).DataType;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
