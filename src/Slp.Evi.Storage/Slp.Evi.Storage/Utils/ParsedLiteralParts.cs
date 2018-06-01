using System;

namespace Slp.Evi.Storage.Utils
{
    /// <summary>
    /// Parsed literal parts.
    /// </summary>
    public sealed class ParsedLiteralParts : IEquatable<ParsedLiteralParts>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParsedLiteralParts"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <param name="languageTag">The language tag.</param>
        public ParsedLiteralParts(string value, Uri type, string languageTag)
        {
            LanguageTag = languageTag;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Uri Type { get; }

        /// <summary>
        /// Gets the language tag.
        /// </summary>
        /// <value>The language tag.</value>
        public string LanguageTag { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is ParsedLiteralParts)) return false;
            return Equals((ParsedLiteralParts) obj);
        }

        /// <inheritdoc />
        public bool Equals(ParsedLiteralParts other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value) && Equals(Type, other.Type) && string.Equals(LanguageTag, other.LanguageTag);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LanguageTag != null ? LanguageTag.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ParsedLiteralParts left, ParsedLiteralParts right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ParsedLiteralParts left, ParsedLiteralParts right)
        {
            return !Equals(left, right);
        }
    }
}