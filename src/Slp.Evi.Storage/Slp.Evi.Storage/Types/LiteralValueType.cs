using System;
using Slp.Evi.Storage.Utils;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Slp.Evi.Storage.Types
{
    /// <summary>
    /// Represents literal value.
    /// </summary>
    /// <seealso cref="ILiteralValueType" />
    public class LiteralValueType
        : ILiteralValueType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralValueType"/> class.
        /// </summary>
        /// <param name="literalType">Type of the literal (<c>null</c> for plain literal).</param>
        /// <param name="languageTag">The language tag (<c>null</c> for no language tag).</param>
        public LiteralValueType(Uri literalType, string languageTag)
        {
            if (string.IsNullOrEmpty(languageTag))
            {
                LanguageTag = null;

                if (literalType == null)
                {
                    LiteralType = null;
                    Category = TypeCategories.SimpleLiteral;
                }
                else
                {
                    LiteralType = literalType;
                    Category = TypeToCategory(literalType);
                }
            }
            else
            {
                LanguageTag = languageTag;
                Category = TypeCategories.OtherLiterals;
            }
        }

        /// <summary>
        /// Convert <paramref name="literalType"/> to <see cref="TypeCategories"/>.
        /// </summary>
        private TypeCategories TypeToCategory(Uri literalType)
        {
            switch (literalType.ToCompleteUri())
            {
                case XmlSpecsHelper.XmlSchemaDataTypeDecimal:
                case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                case XmlSpecsHelper.XmlSchemaDataTypeShort:
                case XmlSpecsHelper.XmlSchemaDataTypeByte:
                case XmlSpecsHelper.XmlSchemaDataTypeInt:
                case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                case XmlSpecsHelper.XmlSchemaDataTypeLong:
                case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                    return TypeCategories.NumericLiteral;
                case XmlSpecsHelper.XmlSchemaDataTypeString:
                    return TypeCategories.StringLiteral;
                case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                    return TypeCategories.BooleanLiteral;
                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                    return TypeCategories.DateTimeLiteral;
                default:
                    return TypeCategories.OtherLiterals;
            }
        }

        /// <summary>
        /// Gets the type of the literal.
        /// </summary>
        /// <value>The type of the literal.</value>
        public Uri LiteralType { get; private set; }

        /// <summary>
        /// Gets the language tag.
        /// </summary>
        /// <value>The language tag.</value>
        public string LanguageTag { get; private set; }

        /// <summary>
        /// Creates the literal node.
        /// </summary>
        /// <param name="factory">The factory to be used.</param>
        /// <param name="value">The value.</param>
        public INode CreateLiteralNode(INodeFactory factory, string value)
        {
            if (LanguageTag != null && LiteralType != null)
            {
                throw new InvalidOperationException("Literal term map cannot have both language tag and data type set");
            }
            else if (LanguageTag != null)
            {
                return factory.CreateLiteralNode(value, LanguageTag);
            }
            else if (LiteralType != null)
            {
                return factory.CreateLiteralNode(value, LiteralType);
            }
            else
            {
                return factory.CreateLiteralNode(value);
            }
        }

        /// <inheritdoc />
        public TypeCategories Category { get; }
    }
}
