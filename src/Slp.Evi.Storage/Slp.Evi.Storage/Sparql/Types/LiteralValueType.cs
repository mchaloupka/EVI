using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace Slp.Evi.Storage.Sparql.Types
{
    /// <summary>
    /// Represents literal value.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Types.ILiteralValueType" />
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
            LiteralType = literalType;
            LanguageTag = languageTag;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is IRI.
        /// </summary>
        /// <value><c>true</c> if this instance is IRI; otherwise, <c>false</c>.</value>
        public bool IsIRI => false;

        /// <summary>
        /// Gets a value indicating whether this instance is blank.
        /// </summary>
        /// <value><c>true</c> if this instance is blank; otherwise, <c>false</c>.</value>
        public bool IsBlank => false;

        /// <summary>
        /// Gets a value indicating whether this instance is literal.
        /// </summary>
        /// <value><c>true</c> if this instance is literal; otherwise, <c>false</c>.</value>
        public bool IsLiteral => true;

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
                throw new Exception("Literal term map cannot have both language tag and datatype set");
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
    }
}
