using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Nodes;

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
        public static IValuedNode Node(this VDS.RDF.Query.Expressions.Primary.ConstantTerm term)
        {
            var property = typeof(VDS.RDF.Query.Expressions.Primary.ConstantTerm).GetProperty("Node", BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Default | BindingFlags.Instance);
            return (IValuedNode)property.GetValue(term);
        }
    }
}
