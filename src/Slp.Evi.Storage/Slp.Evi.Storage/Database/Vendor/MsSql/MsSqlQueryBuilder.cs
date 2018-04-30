using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Database.Base;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;
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
        protected override void GetCommonTypeForComparison(DataType leftDataType, DataType rightDataType, out string neededCastLeft, out string neededCastRight)
        {
            neededCastLeft = null;
            neededCastRight = null;

            if (leftDataType.TypeName == rightDataType.TypeName)
            {
                return;
            }

            var leftTypeName = leftDataType.TypeName.ToLowerInvariant();
            var rightTypeName = rightDataType.TypeName.ToLowerInvariant();

            if (leftDataType.IsNumeric && rightDataType.IsNumeric)
            {
                if ((leftTypeName == "float") || (leftTypeName == "real")
                    || (rightTypeName == "float") || (rightTypeName == "real"))
                {
                    if (leftTypeName != "float" && leftTypeName != "real")
                        neededCastLeft = "float";
                    if (rightTypeName != "float" && rightTypeName != "real")
                        neededCastRight = "float";

                    return;
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
                    var neededType = intTypes[Math.Min(leftIndex, rightIndex)];

                    if (leftIndex < 0)
                        neededCastLeft = neededType;

                    if (rightIndex < 0)
                        neededCastRight = neededType;

                    return;
                }

                if(!leftDataType.IsNumeric)
                    neededCastLeft = "numeric(38,8)";
                if(!rightDataType.IsNumeric)
                    neededCastRight = "numeric(38,8)";

                return;
            }
            else if (leftDataType.IsDateTime && rightDataType.IsDateTime)
            {
                if(!leftDataType.IsDateTime)
                    neededCastLeft = "datetime2";

                if (!rightDataType.IsDateTime)
                    neededCastRight = "datetime2";

                return;
            }

            if (!leftDataType.IsString)
                neededCastLeft = "nvarchar(MAX)";
            if (!rightDataType.IsString)
                neededCastRight = "nvarchar(MAX)";

            return;
        }

        private bool IsNumericType(DataType dataType)
        {
            throw new NotImplementedException();
        }
    }
}
