using System;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlColumnType
    {
        private readonly string _dbType;

        public MsSqlColumnType(string dbType)
        {
            _dbType = dbType;
        }

        public LiteralValueType DefaultRdfType
        {
            get
            {
                switch (_dbType.ToLowerInvariant())
                {
                    case "nvarchar":
                    case "varchar":
                    case "nchar":
                    case "char":
                    case "text":
                    case "ntext":
                        return LiteralValueType.DefaultType;
                    case "bigint":
                    case "int":
                    case "smallint":
                    case "tinyint":
                        return KnownTypes.xsdInteger;
                    case "smallmoney":
                    case "decimal":
                    case "money":
                    case "numeric":
                        return KnownTypes.xsdDecimal;
                    case "bit":
                        return KnownTypes.xsdBoolean;
                    case "float":
                    case "real":
                        return KnownTypes.xsdDouble;
                    case "date":
                        return KnownTypes.xsdDate;
                    case "time":
                        return KnownTypes.xsdTime;
                    case "datetime":
                    case "datetime2":
                        return KnownTypes.xsdDateTime;
                    case "binary":
                    case "varbinary":
                    case "image":
                        return KnownTypes.xsdHexBinary;
                    default:
                        throw new NotImplementedException($"DbType ${_dbType} is not yet supported for detecting default RDF type");
                }
            }
        }
    }
}
