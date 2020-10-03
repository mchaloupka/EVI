using System;
using Slp.Evi.Storage.Core.Common.Database;
using Slp.Evi.Storage.Core.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public abstract class MsSqlColumnType
        : ISqlColumnType
    {
        public abstract LiteralValueType DefaultRdfType { get; }

        public abstract MsSqlColumnType GetCommonType(MsSqlColumnType otherType);

        public static MsSqlColumnType Create(string databaseType)
        {
            var dbType = databaseType.ToLowerInvariant();

            if (IntegerMsSqlColumnType.TryCreate(dbType, out var integerType))
            {
                return integerType;
            }

            if (FloatingPointMsSqlColumnType.TryCreate(dbType, out var floatingType))
            {
                return floatingType;
            }

            if (DecimalMsSqlColumnType.TryCreate(dbType, out var decimalType))
            {
                return decimalType;
            }

            if (DateTimeMsSqlColumnType.TryCreate(dbType, out var dateTimeType))
            {
                return dateTimeType;
            }

            if (VarCharMsSqlColumnType.TryCreate(dbType, out var varcharType))
            {
                return varcharType;
            }

            if (TextMsSqlColumnType.TryCreate(dbType, out var textType))
            {
                return textType;
            }

            throw new NotImplementedException($"The database type \'{dbType}\' is not yet supported");
        }

        public abstract string DbString { get; }
    }

    public abstract class NumericMsSqlColumnType
        : MsSqlColumnType
    { }

    public class IntegerMsSqlColumnType
        : NumericMsSqlColumnType
    {
        public IntegerTypes IntegerType { get; }

        public IntegerMsSqlColumnType(IntegerTypes integerType)
        {
            IntegerType = integerType;
        }

        public enum IntegerTypes
        {
            Bit,
            TinyInt,
            SmallInt,
            Int,
            BigInt
        }

        public static bool TryCreate(string dbType, out IntegerMsSqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "bit":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.Bit);
                    return true;
                case "tinyint":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.TinyInt);
                    return true;
                case "smallint":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.SmallInt);
                    return true;
                case "int":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.Int);
                    return true;
                case "bigint":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.BigInt);
                    return true;
                default:
                    returnValue = null;
                    return false;
            }
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => IntegerType == IntegerTypes.Bit ? KnownTypes.xsdBoolean : KnownTypes.xsdInteger;

        /// <inheritdoc />
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            if (otherType is NullMsSqlColumnType)
            {
                return this;
            }

            if (otherType is TextMsSqlColumnType)
            {
                return otherType;
            }

            if (otherType is IntegerMsSqlColumnType otherIntegerType)
            {
                var t = IntegerType > otherIntegerType.IntegerType ? IntegerType : otherIntegerType.IntegerType;
                return new IntegerMsSqlColumnType(t);
            }

            if (otherType is FloatingPointMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.MaxPrecisionType;
            }

            if (otherType is DecimalMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.MaxPrecisionType;
            }

            return VarCharMsSqlColumnType.NVarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => IntegerType.ToString().ToLowerInvariant();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is IntegerMsSqlColumnType otherIntegerType)
            {
                return IntegerType == otherIntegerType.IntegerType;
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) IntegerType;
        }
    }

    public class FloatingPointMsSqlColumnType
        : NumericMsSqlColumnType
    {
        public FloatingTypes FloatingType { get; }

        public FloatingPointMsSqlColumnType(FloatingTypes floatingType)
        {
            FloatingType = floatingType;
        }

        public enum FloatingTypes
        {
            Real,
            Float
        }

        public static bool TryCreate(string dbType, out FloatingPointMsSqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "real":
                    returnValue = new FloatingPointMsSqlColumnType(FloatingTypes.Real);
                    return true;
                case "float":
                    returnValue = new FloatingPointMsSqlColumnType(FloatingTypes.Float);
                    return true;
                default:
                    returnValue = null;
                    return false;
            }
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => KnownTypes.xsdDouble;

        /// <inheritdoc />
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            if (otherType is NullMsSqlColumnType)
            {
                return this;
            }

            if (otherType is TextMsSqlColumnType)
            {
                return otherType;
            }

            if (otherType is FloatingPointMsSqlColumnType otherFloatingType)
            {
                var t = FloatingType > otherFloatingType.FloatingType ? FloatingType : otherFloatingType.FloatingType;
                return new FloatingPointMsSqlColumnType(t);
            }

            if (otherType is IntegerMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.MaxPrecisionType;
            }

            if (otherType is DecimalMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.MaxPrecisionType;
            }

            return VarCharMsSqlColumnType.NVarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => FloatingType.ToString().ToLowerInvariant();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FloatingPointMsSqlColumnType otherFloating)
            {
                return FloatingType == otherFloating.FloatingType;
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) FloatingType;
        }
    }

    public class DecimalMsSqlColumnType
        : NumericMsSqlColumnType
    {
        public int Precision { get; }

        public int Scale { get; }

        public DecimalMsSqlColumnType(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public static bool TryCreate(string dbType, out DecimalMsSqlColumnType returnValue)
        {
            returnValue = null;

            const string numericPrefix = "numeric";
            const string decimalPrefix = "decimal";

            string arguments;
            if (dbType.StartsWith(numericPrefix))
            {
                arguments = dbType
                    .Substring(numericPrefix.Length, dbType.Length - numericPrefix.Length)
                    .Trim();
            }
            else if (dbType.StartsWith(decimalPrefix))
            {
                arguments = dbType
                    .Substring(numericPrefix.Length, dbType.Length - numericPrefix.Length)
                    .Trim();
            }
            else
            {
                return false;
            }

            if (!string.IsNullOrEmpty(arguments))
            {
                if (!(arguments[0] == '(' && arguments[arguments.Length - 1] == ')'))
                {
                    throw new InvalidOperationException($"Invalid arguments for decimal MS SQL type: {dbType}");
                }

                var splitArguments = arguments.Substring(1, arguments.Length - 2).Split(',');

                if (splitArguments.Length == 1 && int.TryParse(splitArguments[0], out var precision))
                {
                    returnValue = new DecimalMsSqlColumnType(precision, 0);
                }
                else if (splitArguments.Length == 2 && int.TryParse(splitArguments[0], out precision) && int.TryParse(splitArguments[1], out var scale))
                {
                    returnValue = new DecimalMsSqlColumnType(precision, scale);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid arguments for decimal MS SQL type: {dbType}");
                }
            }
            else
            {
                returnValue = new DecimalMsSqlColumnType(18, 0);
            }

            return true;
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => KnownTypes.xsdDecimal;

        /// <inheritdoc />
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            if (otherType is NullMsSqlColumnType)
            {
                return this;
            }

            if (otherType is TextMsSqlColumnType)
            {
                return otherType;
            }

            if (otherType is FloatingPointMsSqlColumnType)
            {
                return MaxPrecisionType;
            }

            if (otherType is IntegerMsSqlColumnType)
            {
                return MaxPrecisionType;
            }

            if (otherType is DecimalMsSqlColumnType otherDecimalMsSqlColumnType)
            {
                var maxPrecision = Math.Max(Precision, otherDecimalMsSqlColumnType.Precision);
                var maxScale = Math.Max(Scale, otherDecimalMsSqlColumnType.Scale);
                var minScale = Math.Min(Scale, otherDecimalMsSqlColumnType.Scale);
                var precision = maxPrecision + (maxScale - minScale);
                if (precision > 38)
                {
                    precision = 38;
                }
                
                return new DecimalMsSqlColumnType(precision, maxScale);
            }

            return VarCharMsSqlColumnType.NVarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => $"decimal({Precision},{Scale})";

        public static DecimalMsSqlColumnType MaxPrecisionType => new DecimalMsSqlColumnType(38, 8);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is DecimalMsSqlColumnType otherDecimal)
            {
                return Precision == otherDecimal.Precision && Scale == otherDecimal.Scale;
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Precision << 10) + Scale;
        }
    }

    public class DateTimeMsSqlColumnType
        : MsSqlColumnType
    {
        public DateTimeTypes DateTimeType { get; }

        public DateTimeMsSqlColumnType(DateTimeTypes dateTimeTypes)
        {
            DateTimeType = dateTimeTypes;
        }

        public enum DateTimeTypes
        {
            Date,
            Time,
            DateTime,
            DateTime2
        }

        public static bool TryCreate(string dbType, out DateTimeMsSqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "date":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.Date);
                    return true;
                case "time":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.Time);
                    return true;
                case "datetime":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.DateTime);
                    return true;
                case "datetime2":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.DateTime2);
                    return true;
                default:
                    returnValue = null;
                    return false;
            }
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType {
            get
            {
                switch (DateTimeType)
                {
                    case DateTimeTypes.Date:
                        return KnownTypes.xsdDate;
                    case DateTimeTypes.Time:
                        return KnownTypes.xsdTime;
                    case DateTimeTypes.DateTime:
                    case DateTimeTypes.DateTime2:
                        return KnownTypes.xsdDateTime;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc />
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            if (otherType is NullMsSqlColumnType)
            {
                return this;
            }

            if (otherType is TextMsSqlColumnType)
            {
                return otherType;
            }

            if (otherType is DateTimeMsSqlColumnType otherDateTimeType)
            {
                if (DateTimeType != otherDateTimeType.DateTimeType)
                {
                    return new DateTimeMsSqlColumnType(DateTimeTypes.DateTime2);
                }
                else
                {
                    return this;
                }
            }

            return VarCharMsSqlColumnType.NVarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => DateTimeType.ToString().ToLowerInvariant();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is DateTimeMsSqlColumnType otherDateTime)
            {
                return DateTimeType == otherDateTime.DateTimeType;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) DateTimeType;
        }
    }

    public abstract class TextualMsSqlColumnType
        : MsSqlColumnType
    { }

    public class VarCharMsSqlColumnType
        : TextualMsSqlColumnType
    {
        public bool IsVar { get; }

        public bool IsUnicode { get; }

        public int? MaxLength { get; }

        public VarCharMsSqlColumnType(bool isVar, bool isUnicode, int? maxLength)
        {
            IsVar = isVar;
            IsUnicode = isUnicode;
            MaxLength = maxLength;
        }

        public static bool TryCreate(string dbType, out VarCharMsSqlColumnType returnValue)
        {
            bool isVar;
            bool isUnicode;

            var curDbType = dbType;
            if (curDbType.StartsWith("n"))
            {
                isUnicode = true;
                curDbType = curDbType.Substring(1);
            }
            else
            {
                isUnicode = false;
            }

            if (curDbType.StartsWith("var"))
            {
                isVar = true;
                curDbType = curDbType.Substring(3);
            }
            else
            {
                isVar = false;
            }

            if (curDbType.StartsWith("char"))
            {
                curDbType = curDbType.Substring(4);
                if (curDbType.Length > 0 && curDbType[0] == '(' && curDbType[curDbType.Length - 1] == ')')
                {
                    var lengthString = curDbType.Substring(1, curDbType.Length - 2);

                    int? maxLength;
                    if (int.TryParse(lengthString, out var length))
                    {
                        maxLength = length;
                    }
                    else if (lengthString == "max")
                    {
                        maxLength = null;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid argument for char type: {dbType}");
                    }

                    returnValue = new VarCharMsSqlColumnType(isUnicode, isVar, maxLength);
                    return true;
                }

                if (curDbType.Length == 0)
                {
                    returnValue = new VarCharMsSqlColumnType(isUnicode, isVar, null);
                    return true;
                }

                throw new InvalidOperationException($"Invalid char type: {dbType}");
            }
            else
            {
                returnValue = null;
                return false;
            }
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => LiteralValueType.DefaultType;

        /// <inheritdoc />
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            if (otherType is NullMsSqlColumnType)
            {
                return this;
            }

            if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(IsUnicode || otherTextMsSqlColumnType.IsUnicode);
            }

            if (otherType is VarCharMsSqlColumnType otherVarCharMsSqlColumnType)
            {
                var newMaxLength = MaxLength.HasValue && otherVarCharMsSqlColumnType.MaxLength.HasValue
                    ? (int?)Math.Max(MaxLength.Value, otherVarCharMsSqlColumnType.MaxLength.Value)
                    : null;

                return new VarCharMsSqlColumnType(IsVar || otherVarCharMsSqlColumnType.IsVar,
                    IsUnicode || otherVarCharMsSqlColumnType.IsUnicode, newMaxLength);
            }

            return NVarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString =>
            $"{(IsUnicode ? "n" : "")}{(IsVar ? "var" : "")}char({(MaxLength.HasValue ? MaxLength.Value.ToString() : "max")})";

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is VarCharMsSqlColumnType otherVarCharMsSqlColumnType)
            {
                return IsUnicode == otherVarCharMsSqlColumnType.IsUnicode &&
                       IsVar == otherVarCharMsSqlColumnType.IsVar &&
                       MaxLength == otherVarCharMsSqlColumnType.MaxLength;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ((IsUnicode ? 1 : 0) << 10)
                   + ((IsVar ? 1 : 0) << 9)
                   + (MaxLength ?? 0);
        }

        public static VarCharMsSqlColumnType NVarCharMaxType =>
            new VarCharMsSqlColumnType(true, true, null);
    }

    public class TextMsSqlColumnType
        : TextualMsSqlColumnType
    {
        public bool IsUnicode { get; }

        public TextMsSqlColumnType(bool isUnicode)
        {
            IsUnicode = isUnicode;
        }

        public static bool TryCreate(string dbType, out TextMsSqlColumnType returnValue)
        {
            bool isUnicode;

            var curDbType = dbType;
            if (curDbType.StartsWith("n"))
            {
                isUnicode = true;
                curDbType = curDbType.Substring(1);
            }
            else
            {
                isUnicode = false;
            }

            if (curDbType == "text")
            {
                returnValue = new TextMsSqlColumnType(isUnicode);
                return true;
            }

            returnValue = null;
            return false;
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => LiteralValueType.DefaultType;

        /// <inheritdoc />
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(IsUnicode || otherTextMsSqlColumnType.IsUnicode);
            }
            else if (otherType is VarCharMsSqlColumnType otherVarCharSqlColumnType)
            {
                return new TextMsSqlColumnType(IsUnicode || otherVarCharSqlColumnType.IsUnicode);
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc />
        public override string DbString =>
            $"{(IsUnicode ? "n" : "")}text";

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is TextMsSqlColumnType otherText)
            {
                return IsUnicode == otherText.IsUnicode;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return IsUnicode ? 1 : 0;
        }
    }

    public class NullMsSqlColumnType
        : MsSqlColumnType
    {
        private NullMsSqlColumnType() { }

        public static NullMsSqlColumnType Instance => new NullMsSqlColumnType();

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => throw new NotSupportedException("Default RDF type is not allowed for NULL type");

        /// <inheritdoc />
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            return otherType;
        }

        /// <inheritdoc />
        public override string DbString => throw new InvalidOperationException("Should not ever be needed to write NULL SQL type");

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is NullMsSqlColumnType;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
