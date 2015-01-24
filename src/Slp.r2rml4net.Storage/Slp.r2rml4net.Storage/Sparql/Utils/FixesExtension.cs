using System.Reflection;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace Slp.r2rml4net.Storage.Sparql.Utils
{
    /// <summary>
    /// Fixes for SPARQL
    /// </summary>
    public static class FixesExtension
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
    }
}
