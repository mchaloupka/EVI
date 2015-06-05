using System.Collections.Generic;
using Slp.r2rml4net.Storage.Sparql.Old.Operator;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sparql.Old
{
    /// <summary>
    /// SPARQL query
    /// </summary>
    public interface ISparqlQuery : IVisitable<ISparqlQueryVisitor>
    {
        /// <summary>
        /// Gets the inner queries.
        /// </summary>
        /// <returns>The inner queries.</returns>
        IEnumerable<ISparqlQuery> GetInnerQueries();

        /// <summary>
        /// Replaces the inner query.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="newQuery">The new query.</param>
        void ReplaceInnerQuery(ISparqlQuery originalQuery, ISparqlQuery newQuery);

        /// <summary>
        /// Finalizes after transform.
        /// </summary>
        /// <returns>The finalized query.</returns>
        ISparqlQuery FinalizeAfterTransform();
    }

    /// <summary>
    /// SPARQL query part
    /// </summary>
    public interface ISparqlQueryPart : ISparqlQuery
    {
        
    }

    /// <summary>
    /// SPARQL query modifier
    /// </summary>
    public interface ISparqlQueryModifier : ISparqlQuery
    {
        /// <summary>
        /// Gets the inner query.
        /// </summary>
        /// <value>The inner query.</value>
        ISparqlQuery InnerQuery { get; }
    }
}
