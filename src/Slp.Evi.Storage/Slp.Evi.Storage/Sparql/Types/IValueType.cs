using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace Slp.Evi.Storage.Sparql.Types
{
    /// <summary>
    /// Type categories for <see cref="IValueType"/>
    /// </summary>
    public enum TypeCategories
    {
        /// <summary>
        /// Represents blank nodes
        /// </summary>
        BlankNode = 0,
        /// <summary>
        /// Represent IRI nodes
        /// </summary>
        IRI = 1,
        /// <summary>
        /// Represent simple literals (without types)
        /// </summary>
        SimpleLiteral = 2,
        /// <summary>
        /// Represent numeric literals
        /// </summary>
        NumericLiteral = 3,
        /// <summary>
        /// Represent string literals
        /// </summary>
        StringLiteral = 4,
        /// <summary>
        /// Represent boolean literal
        /// </summary>
        BooleanLiteral = 5,
        /// <summary>
        /// Represent date-time literal
        /// </summary>
        DateTimeLiteral = 6,
        /// <summary>
        /// Represent other literals
        /// </summary>
        OtherLiterals = 7
    }

    /// <summary>
    /// Interface for type information
    /// </summary>
    public interface IValueType
    {
        /// <summary>
        /// Gets the type category.
        /// </summary>
        TypeCategories Category { get; }
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
        /// <param name="factory">The factory to be used.</param>
        /// <param name="value">The value.</param>
        INode CreateLiteralNode(INodeFactory factory, string value);
    }
}
