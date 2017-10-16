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
