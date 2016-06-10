using System.Collections.Generic;
using System.Diagnostics;
using VDS.RDF.Query.Patterns;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Triple pattern
    /// </summary>
    public class TriplePattern
        : IGraphPattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriplePattern"/> class.
        /// </summary>
        /// <param name="subjectPattern">The subject pattern.</param>
        /// <param name="predicatePattern">The predicate pattern.</param>
        /// <param name="objectPattern">The object pattern.</param>
        public TriplePattern(PatternItem subjectPattern, PatternItem predicatePattern, PatternItem objectPattern)
        {
            SubjectPattern = subjectPattern;
            PredicatePattern = predicatePattern;
            ObjectPattern = objectPattern;

            var variables = new List<string>();

            AddToVariables(SubjectPattern, variables);
            AddToVariables(PredicatePattern, variables);
            AddToVariables(ObjectPattern, variables);

            Variables = variables;
        }

        /// <summary>
        /// Adds to variables list.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="variables">The variables list.</param>
        private void AddToVariables(PatternItem pattern, List<string> variables)
        {
            var variablePattern = pattern as VariablePattern;
            if (variablePattern != null)
            {
                variables.Add(variablePattern.VariableName);
            }
        }

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
        public object Accept(IGraphPatternVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the subject pattern.
        /// </summary>
        /// <value>The subject pattern.</value>
        public PatternItem SubjectPattern { get; }

        /// <summary>
        /// Gets the predicate pattern.
        /// </summary>
        /// <value>The predicate pattern.</value>
        public PatternItem PredicatePattern { get; }

        /// <summary>
        /// Gets the object pattern.
        /// </summary>
        /// <value>The object pattern.</value>
        public PatternItem ObjectPattern { get; }

        /// <summary>
        /// Gets the set of always bound variables.
        /// </summary>
        public IEnumerable<string> AlwaysBoundVariables => Variables;
    }
}
