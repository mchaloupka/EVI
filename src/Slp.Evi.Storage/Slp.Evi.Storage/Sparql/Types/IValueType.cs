using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace Slp.Evi.Storage.Sparql.Types
{
    /// <summary>
    /// Interface for type information
    /// </summary>
    public interface IValueType
    {
        /// <summary>
        /// Gets a value indicating whether this instance is IRI.
        /// </summary>
        /// <value><c>true</c> if this instance is IRI; otherwise, <c>false</c>.</value>
        bool IsIRI { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is blank.
        /// </summary>
        /// <value><c>true</c> if this instance is blank; otherwise, <c>false</c>.</value>
        bool IsBlank { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is literal.
        /// </summary>
        /// <value><c>true</c> if this instance is literal; otherwise, <c>false</c>.</value>
        bool IsLiteral { get; }
    }

    /// <summary>
    /// Interface for literal type information
    /// </summary>
    public interface ILiteralValueType
        : IValueType
    {
        /// <summary>
        /// Gets the type of the literal.
        /// </summary>
        /// <value>The type of the literal.</value>
        Uri LiteralType { get; }

        /// <summary>
        /// Gets the language tag.
        /// </summary>
        /// <value>The language tag.</value>
        string LanguageTag { get; }

        /// <summary>
        /// Creates the literal node.
        /// </summary>
        /// <param name="fact">The factory to be used.</param>
        /// <param name="value">The value.</param>
        INode CreateLiteralNode(INodeFactory fact, string value);
    }
}
