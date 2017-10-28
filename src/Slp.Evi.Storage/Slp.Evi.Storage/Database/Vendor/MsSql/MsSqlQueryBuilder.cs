using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Database.Base;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

namespace Slp.Evi.Storage.Database.Vendor.MsSql
{
    /// <summary>
    /// The implementation of specifics for MS SQL Query builder.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Database.Base.BaseSqlQueryBuilder" />
    public class MsSqlQueryBuilder
        : BaseSqlQueryBuilder
    {
        /// <inheritdoc />
        protected override string GetCommonTypeForComparison(DataType leftDataType, DataType rightDataType)
        {
            if (leftDataType.TypeName == rightDataType.TypeName)
            {
                return leftDataType.TypeName;
            }

            var leftTypeName = leftDataType.TypeName.ToLowerInvariant();
            var rightTypeName = rightDataType.TypeName.ToLowerInvariant();

            if (leftDataType.IsNumeric && rightDataType.IsNumeric)
            {
                if ((leftTypeName == "float") || (leftTypeName == "real")
                    || (rightTypeName == "float") || (rightTypeName == "real"))
                {
                    return "float";
                }

                var intTypes = new List<string>()
                {
                    "bit",
                    "tinyint",
                    "smallint",
                    "int",
                    "bigint"
                };

                var leftIndex = intTypes.FindIndex(x => x == leftTypeName);
                var rightIndex = intTypes.FindIndex(x => x == rightTypeName);

                if (leftIndex >= 0 && rightIndex >= 0)
                {
                    return intTypes[Math.Min(leftIndex, rightIndex)];
                }

                return "numeric(38,8)";
            }
            else if (leftDataType.IsDateTime || rightDataType.IsDateTime)
            {
                return "datetime2";
            }

            return "nvarchar(MAX)";
        }

        private bool IsNumericType(DataType dataType)
        {
            throw new NotImplementedException();
        }
    }
}
