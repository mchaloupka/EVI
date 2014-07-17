using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    /// <summary>
    /// BGP Operator
    /// </summary>
    [DebuggerDisplay("BGP")]
    public class BgpOp : ISparqlQueryPart
    {
        /// <summary>
        /// The predicate pattern
        /// </summary>
        private PatternItem predicatePattern;

        /// <summary>
        /// The object pattern
        /// </summary>
        private PatternItem objectPattern;

        /// <summary>
        /// The subject pattern
        /// </summary>
        private PatternItem subjectPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="BgpOp"/> class.
        /// </summary>
        /// <param name="subjectPattern">The subject pattern.</param>
        /// <param name="predicatePattern">The predicate pattern.</param>
        /// <param name="objectPattern">The object pattern.</param>
        public BgpOp(PatternItem subjectPattern, PatternItem predicatePattern, PatternItem objectPattern)
        {
            this.objectPattern = objectPattern;
            this.predicatePattern = predicatePattern;
            this.subjectPattern = subjectPattern;
        }

        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield break;
        }

        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="q">The q.</param>
        /// <param name="processed">The processed.</param>
        /// <exception cref="System.Exception">Should not be called, BgpOp has no subqueries</exception>
        public void ReplaceInnerQuery(ISparqlQuery q, ISparqlQuery processed)
        {
            throw new Exception("Should not be called, BgpOp has no subqueries");
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>BgpOp.</returns>
        public BgpOp Clone()
        {
            return new BgpOp(subjectPattern, predicatePattern, objectPattern);
        }

        /// <summary>
        /// Gets the predicate pattern.
        /// </summary>
        /// <value>The predicate pattern.</value>
        public PatternItem PredicatePattern { get { return predicatePattern; } }

        /// <summary>
        /// Gets the object pattern.
        /// </summary>
        /// <value>The object pattern.</value>
        public PatternItem ObjectPattern { get { return objectPattern; } }

        /// <summary>
        /// Gets the subject pattern.
        /// </summary>
        /// <value>The subject pattern.</value>
        public PatternItem SubjectPattern { get { return subjectPattern; } }

        /// <summary>
        /// Gets or sets the R2RML graph map.
        /// </summary>
        /// <value>The R2RML graph map.</value>
        public TCode.r2rml4net.Mapping.IGraphMap R2RMLGraphMap { get; set; }

        /// <summary>
        /// Gets or sets the R2RML object map.
        /// </summary>
        /// <value>The R2RML object map.</value>
        public TCode.r2rml4net.Mapping.IObjectMap R2RMLObjectMap { get; set; }

        /// <summary>
        /// Gets or sets the R2RML subject map.
        /// </summary>
        /// <value>The R2RML subject map.</value>
        public TCode.r2rml4net.Mapping.ISubjectMap R2RMLSubjectMap { get; set; }

        /// <summary>
        /// Gets or sets the R2RML triple definition.
        /// </summary>
        /// <value>The R2RML triple definition.</value>
        public TCode.r2rml4net.Mapping.ITriplesMap R2RMLTripleDef { get; set; }

        /// <summary>
        /// Gets or sets the R2RML reference object map.
        /// </summary>
        /// <value>The R2RML reference object map.</value>
        public TCode.r2rml4net.Mapping.IRefObjectMap R2RMLRefObjectMap { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "BGP";
        }

        /// <summary>
        /// Gets or sets the R2RML predicate map.
        /// </summary>
        /// <value>The R2RML predicate map.</value>
        public TCode.r2rml4net.Mapping.IPredicateMap R2RMLPredicateMap { get; set; }


        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        public ISparqlQuery FinalizeAfterTransform()
        {
            return this;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISparqlQueryVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
