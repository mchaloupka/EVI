using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Sparql.Algebra.Modifiers
{
    /// <summary>
    /// The ORDER BY modifier
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.IModifier" />
    public class OrderByModifier
        : IModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByModifier"/> class.
        /// </summary>
        /// <param name="innerQuery">The inner query.</param>
        /// <param name="sparqlVariables">The sparql variables.</param>
        /// <param name="ordering">The ordering</param>
        public OrderByModifier(ISparqlQuery innerQuery, IEnumerable<string> sparqlVariables, IEnumerable<OrderingPart> ordering)
        {
            InnerQuery = innerQuery;
            Variables = sparqlVariables;
            Ordering = ordering;
        }

        /// <summary>
        /// Gets the ordering.
        /// </summary>
        public IEnumerable<OrderingPart> Ordering { get; }

        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        public ISparqlQuery InnerQuery { get; }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IModifierVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Representation of a part in ordering
        /// </summary>
        public class OrderingPart
        {
            /// <summary>
            /// Gets the variable.
            /// </summary>
            public string Variable { get;}

            /// <summary>
            /// Gets a value indicating whether this variable has DESC ordering.
            /// </summary>
            public bool IsDescending { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="OrderingPart"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="isDescending">A value indicating whether the <paramref name="variable"/> has DESC ordering.</param>
            public OrderingPart(string variable, bool isDescending)
            {
                Variable = variable;
                IsDescending = isDescending;
            }
        }
    }
}
