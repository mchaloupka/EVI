using System;
using System.Linq;
using System.Reflection;
using TCode.r2rml4net.Extensions;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace Slp.Evi.Storage.Utils
{
    /// <summary>
    /// Extension class to fix issues in the used libraries
    /// </summary>
    public static class FixesExtensions
    {
        /// <summary>
        /// Gets the node for the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <returns>The node.</returns>
        public static IValuedNode Node(this ConstantTerm term)
        {
            var property = typeof(ConstantTerm).GetProperty("Node", BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Default | BindingFlags.Instance);
            return (IValuedNode)property.GetValue(term);
        }

        /// <summary>
        /// Gets the specified literal term map (parsed).
        /// </summary>
        public static ParsedLiteralParts Parsed(this ILiteralTermMap literalTermMap)
        {
            var literalNode = literalTermMap.Node.GetObjects("rr:constant").FirstOrDefault() as ILiteralNode;

            if (literalNode != null)
            {
                return new ParsedLiteralParts(literalNode.Value, literalNode.DataType, literalNode.Language);
            }
            else
            {
                throw new Exception("Cannot get the constant");
            }
        }
    }
}
