using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query.Source;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using System.Diagnostics;
using Slp.r2rml4net.Storage.Mapping;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Triple pattern
    /// </summary>
    public class RestrictedTriplePattern
        : IGraphPattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictedTriplePattern"/> class.
        /// </summary>
        /// <param name="subjectPattern">The subject pattern.</param>
        /// <param name="predicatePattern">The predicate pattern.</param>
        /// <param name="objectPattern">The object pattern.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="objectMap">The object map.</param>
        /// <param name="refObjectMap">The reference object map</param>
        /// <param name="graphMap">The graph map.</param>
        public RestrictedTriplePattern(PatternItem subjectPattern, PatternItem predicatePattern, 
            PatternItem objectPattern, ITriplesMap tripleMap, ISubjectMap subjectMap, 
            IPredicateMap predicateMap, IObjectMap objectMap, IRefObjectMap refObjectMap,
            IGraphMap graphMap)
        {
            SubjectPattern = subjectPattern;
            PredicatePattern = predicatePattern;
            ObjectPattern = objectPattern;

            TripleMap = tripleMap;
            SubjectMap = subjectMap;
            PredicateMap = predicateMap;
            ObjectMap = objectMap;
            RefObjectMap = refObjectMap;
            GraphMap = graphMap;

            var variables = new List<string>();

            AddToVariables(SubjectPattern, variables);
            AddToVariables(PredicatePattern, variables);
            AddToVariables(ObjectPattern, variables);

            Variables = variables;
        }

        /// <summary>
        /// Gets the graph map.
        /// </summary>
        /// <value>The graph map.</value>
        public IGraphMap GraphMap { get; private set; }

        /// <summary>
        /// Gets the object map.
        /// </summary>
        /// <value>The object map.</value>
        public IObjectMap ObjectMap { get; private set; }

        /// <summary>
        /// Gets the reference object map.
        /// </summary>
        /// <value>The reference object map.</value>
        public IRefObjectMap RefObjectMap { get; private set; }

        /// <summary>
        /// Gets the predicate map.
        /// </summary>
        /// <value>The predicate map.</value>
        public IPredicateMap PredicateMap { get; private set; }

        /// <summary>
        /// Gets or sets the subject map.
        /// </summary>
        /// <value>The subject map.</value>
        public ISubjectMap SubjectMap { get; private set; }

        /// <summary>
        /// Gets the triple map.
        /// </summary>
        /// <value>The triple map.</value>
        public ITriplesMap TripleMap { get; private set; }

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
        public IEnumerable<string> Variables { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IPatternVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the subject pattern.
        /// </summary>
        /// <value>The subject pattern.</value>
        public PatternItem SubjectPattern { get; private set; }

        /// <summary>
        /// Gets the predicate pattern.
        /// </summary>
        /// <value>The predicate pattern.</value>
        public PatternItem PredicatePattern { get; private set; }

        /// <summary>
        /// Gets the object pattern.
        /// </summary>
        /// <value>The object pattern.</value>
        public PatternItem ObjectPattern { get; private set; }
    }
}
