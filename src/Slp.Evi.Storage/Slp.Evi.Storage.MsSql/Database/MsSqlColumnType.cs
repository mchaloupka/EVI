using System;
using System.ComponentModel;
using Slp.Evi.Common.Database;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public abstract class MsSqlColumnType
        : ISqlColumnType
    {
        protected MsSqlColumnType(bool isNullable)
        {
            IsNullable = isNullable;
        }

        public abstract LiteralValueType DefaultRdfType { get; }

        public abstract MsSqlColumnType GetCommonType(MsSqlColumnType otherType);

        /// <inheritdoc />
        public bool IsNullable { get; }

        public static MsSqlColumnType Create(string databaseType, bool isNullable)
        {
            var dbType = databaseType.ToLowerInvariant();

            if (IntegerMsSqlColumnType.TryCreate(dbType, isNullable, out var integerType))
            {
                return integerType;
            }
            else if (FloatingPointMsSqlColumnType.TryCreate(dbType, isNullable, out var floatingType))
            {
                return floatingType;
            }
            else if (DecimalMsSqlColumnType.TryCreate(dbType, isNullable, out var decimalType))
            {
                return decimalType;
            }
            else if (DateTimeMsSqlColumnType.TryCreate(dbType, isNullable, out var dateTimeType))
            {
                return dateTimeType;
            }
            else if (VarCharMsSqlColumnType.TryCreate(dbType, isNullable, out var varcharType))
            {
                return varcharType;
            }
            else if (TextMsSqlColumnType.TryCreate(dbType, isNullable, out var textType))
            {
                return textType;
            }
            else
            {
                throw new NotImplementedException($"The database type \'{dbType}\' is not yet supported");
            }
        }

        public abstract string DbString { get; }
    }

    public abstract class NumericMsSqlColumnType
        : MsSqlColumnType
    {
        protected NumericMsSqlColumnType(bool isNullable)
            : base(isNullable) { }
    }

    public class IntegerMsSqlColumnType
        : NumericMsSqlColumnType
    {
        public IntegerTypes IntegerType { get; }

        public IntegerMsSqlColumnType(IntegerTypes integerType, bool isNullable)
            : base(isNullable)
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

        public static bool TryCreate(string dbType, bool isNullable, out IntegerMsSqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "bit":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.Bit, isNullable);
                    return true;
                case "tinyint":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.TinyInt, isNullable);
                    return true;
                case "smallint":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.SmallInt, isNullable);
                    return true;
                case "int":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.Int, isNullable);
                    return true;
                case "bigint":
                    returnValue = new IntegerMsSqlColumnType(IntegerTypes.BigInt, isNullable);
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
            else if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(otherTextMsSqlColumnType.IsUnicode, IsNullable || otherType.IsNullable);
            }
            else if (otherType is IntegerMsSqlColumnType otherIntegerType)
            {
                var t = IntegerType > otherIntegerType.IntegerType ? IntegerType : otherIntegerType.IntegerType;
                return new IntegerMsSqlColumnType(t, IsNullable || otherType.IsNullable);
            }
            else if (otherType is FloatingPointMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.GetMaxPrecisionType(IsNullable || otherType.IsNullable);
            }
            else if (otherType is DecimalMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.GetMaxPrecisionType(IsNullable || otherType.IsNullable);
            }
            else
            {
                return VarCharMsSqlColumnType.GetNVarCharMax(IsNullable || otherType.IsNullable);
            }
        }

        /// <inheritdoc />
        public override string DbString => IntegerType.ToString().ToLowerInvariant();
    }

    public class FloatingPointMsSqlColumnType
        : NumericMsSqlColumnType
    {
        public FloatingTypes FloatingType { get; }

        public FloatingPointMsSqlColumnType(FloatingTypes floatingType, bool isNullable)
            : base(isNullable)
        {
            FloatingType = floatingType;
        }

        public enum FloatingTypes
        {
            Real,
            Float
        }

        public static bool TryCreate(string dbType, bool isNullable, out FloatingPointMsSqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "real":
                    returnValue = new FloatingPointMsSqlColumnType(FloatingTypes.Real, isNullable);
                    return true;
                case "float":
                    returnValue = new FloatingPointMsSqlColumnType(FloatingTypes.Float, isNullable);
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
            else if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(otherTextMsSqlColumnType.IsUnicode, IsNullable || otherType.IsNullable);
            }
            else if (otherType is FloatingPointMsSqlColumnType otherFloatingType)
            {
                var t = FloatingType > otherFloatingType.FloatingType ? FloatingType : otherFloatingType.FloatingType;
                return new FloatingPointMsSqlColumnType(t, IsNullable || otherType.IsNullable);
            }
            else if (otherType is IntegerMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.GetMaxPrecisionType(IsNullable || otherType.IsNullable);
            }
            else if (otherType is DecimalMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.GetMaxPrecisionType(IsNullable || otherType.IsNullable);
            }
            else
            {
                return VarCharMsSqlColumnType.GetNVarCharMax(IsNullable || otherType.IsNullable);
            }
        }

        /// <inheritdoc />
        public override string DbString => FloatingType.ToString().ToLowerInvariant();
    }

    public class DecimalMsSqlColumnType
        : NumericMsSqlColumnType
    {
        public int Precision { get; }

        public int Scale { get; }

        public DecimalMsSqlColumnType(int precision, int scale, bool isNullable)
            : base(isNullable)
        {
            Precision = precision;
            Scale = scale;
        }

        public static bool TryCreate(string dbType, bool isNullable, out DecimalMsSqlColumnType returnValue)
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
                    returnValue = new DecimalMsSqlColumnType(precision, 0, isNullable);
                }
                else if (splitArguments.Length == 2 && int.TryParse(splitArguments[0], out precision) && int.TryParse(splitArguments[1], out var scale))
                {
                    returnValue = new DecimalMsSqlColumnType(precision, scale, isNullable);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid arguments for decimal MS SQL type: {dbType}");
                }
            }
            else
            {
                returnValue = new DecimalMsSqlColumnType(18, 0, isNullable);
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
            else if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(otherTextMsSqlColumnType.IsUnicode, IsNullable || otherType.IsNullable);
            }
            else if (otherType is FloatingPointMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.GetMaxPrecisionType(IsNullable || otherType.IsNullable);
            }
            else if (otherType is IntegerMsSqlColumnType)
            {
                return DecimalMsSqlColumnType.GetMaxPrecisionType(IsNullable || otherType.IsNullable);
            }
            else if (otherType is DecimalMsSqlColumnType otherDecimalMsSqlColumnType)
            {
                var maxPrecision = Math.Max(Precision, otherDecimalMsSqlColumnType.Precision);
                var maxScale = Math.Max(Scale, otherDecimalMsSqlColumnType.Scale);
                var minScale = Math.Min(Scale, otherDecimalMsSqlColumnType.Scale);
                var precision = maxPrecision + (maxScale - minScale);
                if (precision > 38)
                {
                    precision = 38;
                }
                
                return new DecimalMsSqlColumnType(precision, maxScale, IsNullable || otherType.IsNullable);
            }
            else
            {
                return VarCharMsSqlColumnType.GetNVarCharMax(IsNullable || otherType.IsNullable);
            }
        }

        /// <inheritdoc />
        public override string DbString => $"decimal({Precision},{Scale})";

        public static DecimalMsSqlColumnType GetMaxPrecisionType(bool isNullable)
        {
            return new DecimalMsSqlColumnType(38, 8, isNullable);
        }
    }

    public class DateTimeMsSqlColumnType
        : MsSqlColumnType
    {
        public DateTimeTypes DateTimeType { get; }

        public DateTimeMsSqlColumnType(DateTimeTypes dateTimeTypes, bool isNullable)
            : base(isNullable)
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

        public static bool TryCreate(string dbType, bool isNullable, out DateTimeMsSqlColumnType returnValue)
        {
            switch (dbType)
            {
                case "date":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.Date, isNullable);
                    return true;
                case "time":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.Time, isNullable);
                    return true;
                case "datetime":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.DateTime, isNullable);
                    return true;
                case "datetime2":
                    returnValue = new DateTimeMsSqlColumnType(DateTimeTypes.DateTime2, isNullable);
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
            else if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(otherTextMsSqlColumnType.IsUnicode, IsNullable || otherType.IsNullable);
            }
            else if (otherType is DateTimeMsSqlColumnType otherDateTimeType)
            {
                if (DateTimeType != otherDateTimeType.DateTimeType)
                {
                    return new DateTimeMsSqlColumnType(DateTimeTypes.DateTime2, IsNullable || otherType.IsNullable);
                }
                else
                {
                    return new DateTimeMsSqlColumnType(DateTimeType, IsNullable || otherType.IsNullable);
                }
            }
            else
            {
                return VarCharMsSqlColumnType.GetNVarCharMax(IsNullable || otherType.IsNullable);
            }
        }

        /// <inheritdoc />
        public override string DbString => DateTimeType.ToString().ToLowerInvariant();
    }

    public abstract class TextualMsSqlColumnType
        : MsSqlColumnType
    {
        protected TextualMsSqlColumnType(bool isNullable)
            : base(isNullable) { }
    }

    public class VarCharMsSqlColumnType
        : TextualMsSqlColumnType
    {
        public bool IsVar { get; }
        public bool IsUnicode { get; }
        public int? MaxLength { get; }

        public VarCharMsSqlColumnType(bool isVar, bool isUnicode, int? maxLength, bool isNulable)
            : base(isNulable)
        {
            IsVar = isVar;
            IsUnicode = isUnicode;
            MaxLength = maxLength;
        }

        public static bool TryCreate(string dbType, bool isNullable, out VarCharMsSqlColumnType returnValue)
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

                    returnValue = new VarCharMsSqlColumnType(isUnicode, isVar, maxLength, isNullable);
                    return true;
                }
                else if (curDbType.Length == 0)
                {
                    returnValue = new VarCharMsSqlColumnType(isUnicode, isVar, null, isNullable);
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
        public override MsSqlColumnType GetCommonType(MsSqlColumnType otherType)
        {
            if (otherType is NullMsSqlColumnType)
            {
                return this;
            }
            else if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(otherTextMsSqlColumnType.IsUnicode, IsNullable || otherType.IsNullable);
            }
            else if (otherType is VarCharMsSqlColumnType otherVarCharMsSqlColumnType)
            {
                var newMaxLength = MaxLength.HasValue && otherVarCharMsSqlColumnType.MaxLength.HasValue
                    ? (int?)Math.Max(MaxLength.Value, otherVarCharMsSqlColumnType.MaxLength.Value)
                    : null;

                return new VarCharMsSqlColumnType(IsVar || otherVarCharMsSqlColumnType.IsVar,
                    IsUnicode || otherVarCharMsSqlColumnType.IsUnicode, newMaxLength,
                    IsNullable || otherType.IsNullable);
            }
            else
            {
                return VarCharMsSqlColumnType.GetNVarCharMax(IsNullable || otherType.IsNullable);
            }
        }

        /// <inheritdoc />
        public override string DbString =>
            $"{(IsUnicode ? "n" : "")}{(IsVar ? "var" : "")}char({(MaxLength.HasValue ? MaxLength.Value.ToString() : "max")})";

        public static VarCharMsSqlColumnType GetNVarCharMax(bool isNullable)
        {
            return new VarCharMsSqlColumnType(true, true, null, isNullable);
        }
    }

    public class TextMsSqlColumnType
        : TextualMsSqlColumnType
    {
        public bool IsUnicode { get; }

        public TextMsSqlColumnType(bool isUnicode, bool isNullable)
            : base(isNullable)
        {
            IsUnicode = isUnicode;
        }

        public static bool TryCreate(string dbType, bool isNullable, out TextMsSqlColumnType returnValue)
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
                returnValue = new TextMsSqlColumnType(isUnicode, isNullable);
                return true;
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
            else if (otherType is TextMsSqlColumnType otherTextMsSqlColumnType)
            {
                return new TextMsSqlColumnType(IsUnicode || otherTextMsSqlColumnType.IsUnicode,
                    IsNullable || otherTextMsSqlColumnType.IsNullable);
            }
            else if (otherType is VarCharMsSqlColumnType otherVarCharSqlColumnType)
            {
                return new TextMsSqlColumnType(IsUnicode || otherVarCharSqlColumnType.IsUnicode,
                    IsNullable || otherType.IsNullable);
            }
            else
            {
                return new TextMsSqlColumnType(IsUnicode, IsNullable || otherType.IsNullable);
            }
        }

        /// <inheritdoc />
        public override string DbString =>
            $"{(IsUnicode ? "n" : "")}text";
    }

    public class NullMsSqlColumnType
        : MsSqlColumnType
    {
        private NullMsSqlColumnType(): base(true) { }

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
    }
}
