using System;
using Slp.Evi.Storage.Core.Common.Database;
using Slp.Evi.Storage.Core.Common.Types;

namespace Slp.Evi.Storage.MySql.Database
{
    public abstract class MySqlColumnType
        : ISqlColumnType
    {
        public abstract LiteralValueType DefaultRdfType { get; }

        public abstract MySqlColumnType GetCommonType(MySqlColumnType otherType);

        public static MySqlColumnType Create(string databaseType)
        {
            var dbType = databaseType.ToLowerInvariant();

            if (IntegerMySqlColumnType.TryCreate(dbType, out var integerType))
            {
                return integerType;
            }

            if (FloatingPointMySqlColumnType.TryCreate(dbType, out var floatingType))
            {
                return floatingType;
            }

            if (DecimalMySqlColumnType.TryCreate(dbType, out var decimalType))
            {
                return decimalType;
            }

            if (DateTimeMySqlColumnType.TryCreate(dbType, out var dateTimeType))
            {
                return dateTimeType;
            }

            if (VarCharMySqlColumnType.TryCreate(dbType, out var varcharType))
            {
                return varcharType;
            }

            if (TextMySqlColumnType.TryCreate(dbType, out var textType))
            {
                return textType;
            }

            throw new NotImplementedException($"The database type \'{dbType}\' is not yet supported");
        }

        public abstract string DbString { get; }
    }

    public abstract class NumericMySqlColumnType
        : MySqlColumnType
    { }

    public class IntegerMySqlColumnType
        : NumericMySqlColumnType
    {
        public IntegerTypes IntegerType { get; }

        public IntegerMySqlColumnType(IntegerTypes integerType)
        {
            IntegerType = integerType;
        }

        public enum IntegerTypes
        {
            Bit,
            TinyInt,
            SmallInt,
            MediumInt,
            Int,
            BigInt
        }

        public static bool TryCreate(string dbType, out IntegerMySqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "bit":
                    returnValue = new IntegerMySqlColumnType(IntegerTypes.Bit);
                    return true;
                case "tinyint":
                    returnValue = new IntegerMySqlColumnType(IntegerTypes.TinyInt);
                    return true;
                case "smallint":
                    returnValue = new IntegerMySqlColumnType(IntegerTypes.SmallInt);
                    return true;
                case "mediumint":
                    returnValue = new IntegerMySqlColumnType(IntegerTypes.MediumInt);
                    return true;
                case "int":
                    returnValue = new IntegerMySqlColumnType(IntegerTypes.Int);
                    return true;
                case "bigint":
                    returnValue = new IntegerMySqlColumnType(IntegerTypes.BigInt);
                    return true;
                default:
                    returnValue = null;
                    return false;
            }
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => IntegerType == IntegerTypes.Bit ? KnownTypes.xsdBoolean : KnownTypes.xsdInteger;

        /// <inheritdoc />
        public override MySqlColumnType GetCommonType(MySqlColumnType otherType)
        {
            if (otherType is NullMySqlColumnType)
            {
                return this;
            }

            if (otherType is TextMySqlColumnType)
            {
                return otherType;
            }

            if (otherType is IntegerMySqlColumnType otherIntegerType)
            {
                var t = IntegerType > otherIntegerType.IntegerType ? IntegerType : otherIntegerType.IntegerType;
                return new IntegerMySqlColumnType(t);
            }

            if (otherType is FloatingPointMySqlColumnType)
            {
                return DecimalMySqlColumnType.MaxPrecisionType;
            }

            if (otherType is DecimalMySqlColumnType)
            {
                return DecimalMySqlColumnType.MaxPrecisionType;
            }

            return VarCharMySqlColumnType.VarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => IntegerType.ToString().ToLowerInvariant();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is IntegerMySqlColumnType otherIntegerType)
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

    public class FloatingPointMySqlColumnType
        : NumericMySqlColumnType
    {
        public FloatingTypes FloatingType { get; }

        public FloatingPointMySqlColumnType(FloatingTypes floatingType)
        {
            FloatingType = floatingType;
        }

        public enum FloatingTypes
        {
            Float,
            Double
        }

        public static bool TryCreate(string dbType, out FloatingPointMySqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "float":
                    returnValue = new FloatingPointMySqlColumnType(FloatingTypes.Float);
                    return true;
                case "double":
                    returnValue = new FloatingPointMySqlColumnType(FloatingTypes.Double);
                    return true;
                default:
                    returnValue = null;
                    return false;
            }
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => KnownTypes.xsdDouble;

        /// <inheritdoc />
        public override MySqlColumnType GetCommonType(MySqlColumnType otherType)
        {
            if (otherType is NullMySqlColumnType)
            {
                return this;
            }

            if (otherType is TextMySqlColumnType)
            {
                return otherType;
            }

            if (otherType is FloatingPointMySqlColumnType otherFloatingType)
            {
                var t = FloatingType > otherFloatingType.FloatingType ? FloatingType : otherFloatingType.FloatingType;
                return new FloatingPointMySqlColumnType(t);
            }

            if (otherType is IntegerMySqlColumnType)
            {
                return DecimalMySqlColumnType.MaxPrecisionType;
            }

            if (otherType is DecimalMySqlColumnType)
            {
                return DecimalMySqlColumnType.MaxPrecisionType;
            }

            return VarCharMySqlColumnType.VarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => FloatingType.ToString().ToLowerInvariant();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FloatingPointMySqlColumnType otherFloating)
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

    public class DecimalMySqlColumnType
        : NumericMySqlColumnType
    {
        public int Precision { get; }

        public int Scale { get; }

        public DecimalMySqlColumnType(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public static bool TryCreate(string dbType, out DecimalMySqlColumnType returnValue)
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
                if (!(arguments[0] == '(' && arguments[^1] == ')'))
                {
                    throw new InvalidOperationException($"Invalid arguments for decimal MS SQL type: {dbType}");
                }

                var splitArguments = arguments.Substring(1, arguments.Length - 2).Split(',');

                if (splitArguments.Length == 1 && int.TryParse(splitArguments[0], out var precision))
                {
                    returnValue = new DecimalMySqlColumnType(precision, 0);
                }
                else if (splitArguments.Length == 2 && int.TryParse(splitArguments[0], out precision) && int.TryParse(splitArguments[1], out var scale))
                {
                    returnValue = new DecimalMySqlColumnType(precision, scale);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid arguments for decimal MS SQL type: {dbType}");
                }
            }
            else
            {
                returnValue = new DecimalMySqlColumnType(18, 0);
            }

            return true;
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => KnownTypes.xsdDecimal;

        /// <inheritdoc />
        public override MySqlColumnType GetCommonType(MySqlColumnType otherType)
        {
            if (otherType is NullMySqlColumnType)
            {
                return this;
            }

            if (otherType is TextMySqlColumnType)
            {
                return otherType;
            }

            if (otherType is FloatingPointMySqlColumnType)
            {
                return MaxPrecisionType;
            }

            if (otherType is IntegerMySqlColumnType)
            {
                return MaxPrecisionType;
            }

            if (otherType is DecimalMySqlColumnType otherDecimalMySqlColumnType)
            {
                var maxPrecision = Math.Max(Precision, otherDecimalMySqlColumnType.Precision);
                var maxScale = Math.Max(Scale, otherDecimalMySqlColumnType.Scale);
                var minScale = Math.Min(Scale, otherDecimalMySqlColumnType.Scale);
                var precision = maxPrecision + (maxScale - minScale);
                if (precision > 38)
                {
                    precision = 38;
                }
                
                return new DecimalMySqlColumnType(precision, maxScale);
            }

            return VarCharMySqlColumnType.VarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => $"decimal({Precision},{Scale})";

        public static DecimalMySqlColumnType MaxPrecisionType => new DecimalMySqlColumnType(38, 8);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is DecimalMySqlColumnType otherDecimal)
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

    public class DateTimeMySqlColumnType
        : MySqlColumnType
    {
        public DateTimeTypes DateTimeType { get; }

        public DateTimeMySqlColumnType(DateTimeTypes dateTimeTypes)
        {
            DateTimeType = dateTimeTypes;
        }

        public enum DateTimeTypes
        {
            Date,
            Time,
            DateTime,
            Timestamp,
            Year
        }

        public static bool TryCreate(string dbType, out DateTimeMySqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "date":
                    returnValue = new DateTimeMySqlColumnType(DateTimeTypes.Date);
                    return true;
                case "time":
                    returnValue = new DateTimeMySqlColumnType(DateTimeTypes.Time);
                    return true;
                case "datetime":
                    returnValue = new DateTimeMySqlColumnType(DateTimeTypes.DateTime);
                    return true;
                case "timestamp":
                    returnValue = new DateTimeMySqlColumnType(DateTimeTypes.Timestamp);
                    return true;
                case "year":
                    returnValue = new DateTimeMySqlColumnType(DateTimeTypes.Year);
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
                    case DateTimeTypes.Year:
                        return KnownTypes.xsdDate;
                    case DateTimeTypes.Time:
                        return KnownTypes.xsdTime;
                    case DateTimeTypes.DateTime:
                    case DateTimeTypes.Timestamp:
                        return KnownTypes.xsdDateTime;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc />
        public override MySqlColumnType GetCommonType(MySqlColumnType otherType)
        {
            if (otherType is NullMySqlColumnType)
            {
                return this;
            }

            if (otherType is TextMySqlColumnType)
            {
                return otherType;
            }

            if (otherType is DateTimeMySqlColumnType otherDateTimeType)
            {
                if (DateTimeType != otherDateTimeType.DateTimeType)
                {
                    if (DefaultRdfType == KnownTypes.xsdDate && otherDateTimeType.DefaultRdfType == KnownTypes.xsdDate)
                    {
                        return new DateTimeMySqlColumnType(DateTimeTypes.Date);
                    }
                    else
                    {
                        return new DateTimeMySqlColumnType(DateTimeTypes.DateTime);
                    }
                }
                else
                {
                    return this;
                }
            }

            return VarCharMySqlColumnType.VarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString => DateTimeType.ToString().ToLowerInvariant();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is DateTimeMySqlColumnType otherDateTime)
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

    public abstract class TextualMySqlColumnType
        : MySqlColumnType
    { }

    public class VarCharMySqlColumnType
        : TextualMySqlColumnType
    {
        public bool IsVar { get; }

        public int MaxLength { get; }

        public VarCharMySqlColumnType(bool isVar, int maxLength)
        {
            IsVar = isVar;
            MaxLength = maxLength;
        }

        public static bool TryCreate(string dbType, out VarCharMySqlColumnType returnValue)
        {
            bool isVar;

            var curDbType = dbType;

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
                if (curDbType.Length > 0 && curDbType[0] == '(' && curDbType[^1] == ')')
                {
                    var lengthString = curDbType.Substring(1, curDbType.Length - 2);

                    int maxLength;
                    if (int.TryParse(lengthString, out var length))
                    {
                        maxLength = length;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid argument for char type: {dbType}");
                    }

                    returnValue = new VarCharMySqlColumnType(isVar, maxLength);
                    return true;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid char type: {dbType}");
                }
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
        public override MySqlColumnType GetCommonType(MySqlColumnType otherType)
        {
            if (otherType is NullMySqlColumnType)
            {
                return this;
            }

            if (otherType is TextMySqlColumnType otherTextMySqlColumnType)
            {
                return otherType;
            }

            if (otherType is VarCharMySqlColumnType otherVarCharMySqlColumnType)
            {
                var newMaxLength = Math.Max(MaxLength, otherVarCharMySqlColumnType.MaxLength);
                return new VarCharMySqlColumnType(IsVar || otherVarCharMySqlColumnType.IsVar, newMaxLength);
            }

            return VarCharMaxType;
        }

        /// <inheritdoc />
        public override string DbString =>
            $"{(IsVar ? "var" : "")}char({MaxLength})";

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is VarCharMySqlColumnType otherVarCharMySqlColumnType)
            {
                return IsVar == otherVarCharMySqlColumnType.IsVar &&
                       MaxLength == otherVarCharMySqlColumnType.MaxLength;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ((IsVar ? 1 : 0) << 9)
                   + (MaxLength);
        }

        public static VarCharMySqlColumnType VarCharMaxType =>
            new VarCharMySqlColumnType(true, 65535);
    }

    public class TextMySqlColumnType
        : TextualMySqlColumnType
    {
        public bool IsBlob { get; }
        public TextTypes TextType { get; }

        public TextMySqlColumnType(bool isBlob, TextTypes textType)
        {
            IsBlob = isBlob;
            TextType = textType;
        }

        public enum TextTypes
        {
            Tiny,
            Standard,
            Medium,
            Long
        }

        public static bool TryCreate(string dbType, out TextMySqlColumnType returnValue)
        {
            var curDbType = dbType;

            bool? isBlob = null;

            if (curDbType.EndsWith("text"))
            {
                isBlob = false;
                curDbType = curDbType.Substring(0, curDbType.Length - 4);
            }
            else if (curDbType.EndsWith("blob"))
            {
                isBlob = true;
                curDbType = curDbType.Substring(0, curDbType.Length - 4);
            }

            if (isBlob.HasValue)
            {
                switch (curDbType)
                {
                    case "":
                        returnValue = new TextMySqlColumnType(isBlob.Value, TextTypes.Standard);
                        return true;
                    case "tiny":
                        returnValue = new TextMySqlColumnType(isBlob.Value, TextTypes.Tiny);
                        return true;
                    case "medium":
                        returnValue = new TextMySqlColumnType(isBlob.Value, TextTypes.Medium);
                        return true;
                    case "long":
                        returnValue = new TextMySqlColumnType(isBlob.Value, TextTypes.Long);
                        return true;
                }
            }

            returnValue = null;
            return false;
        }

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => LiteralValueType.DefaultType;

        /// <inheritdoc />
        public override MySqlColumnType GetCommonType(MySqlColumnType otherType)
        {
            if (otherType is TextMySqlColumnType otherTextMySqlColumnType)
            {
                if (TextType > otherTextMySqlColumnType.TextType)
                {
                    return this;
                }
                else
                {
                    return otherTextMySqlColumnType;
                }
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc />
        public override string DbString
        {
            get
            {
                var suffix = IsBlob ? "blob" : "text";

                switch (TextType)
                {
                    case TextTypes.Tiny:
                        return $"tiny${suffix}";
                    case TextTypes.Standard:
                        return suffix;
                    case TextTypes.Medium:
                        return $"medium{suffix}";
                    case TextTypes.Long:
                        return $"long{suffix}";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is TextMySqlColumnType otherText)
            {
                return IsBlob == otherText.IsBlob && TextType == otherText.TextType;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return IsBlob ? 1 : 0 + ((int) TextType) << 1;
        }
    }

    public class NullMySqlColumnType
        : MySqlColumnType
    {
        private NullMySqlColumnType() { }

        public static NullMySqlColumnType Instance => new NullMySqlColumnType();

        /// <inheritdoc />
        public override LiteralValueType DefaultRdfType => throw new NotSupportedException("Default RDF type is not allowed for NULL type");

        /// <inheritdoc />
        public override MySqlColumnType GetCommonType(MySqlColumnType otherType)
        {
            return otherType;
        }

        /// <inheritdoc />
        public override string DbString => throw new InvalidOperationException("Should not ever be needed to write NULL SQL type");

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is NullMySqlColumnType;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
