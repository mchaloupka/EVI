using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Mapping;
using Slp.Evi.Storage.Sparql.Types;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace Slp.Evi.Storage.Query
{
    /// <summary>
    /// The query context.
    /// </summary>
    public interface IQueryContext
    {
        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        ISqlDatabase Db { get; }

        /// <summary>
        /// Gets the mapping.
        /// </summary>
        /// <value>The mapping.</value>
        IMappingProcessor Mapping { get; }

        /// <summary>
        /// Gets the node factory.
        /// </summary>
        /// <value>The node factory.</value>
        INodeFactory NodeFactory { get; }

        /// <summary>
        /// Gets the original algebra.
        /// </summary>
        /// <value>The original algebra.</value>
        ISparqlAlgebra OriginalAlgebra { get; }

        /// <summary>
        /// Gets the original query.
        /// </summary>
        /// <value>The original query.</value>
        SparqlQuery OriginalQuery { get; }

        /// <summary>
        /// Gets the query naming helpers.
        /// </summary>
        /// <value>The query naming helpers.</value>
        QueryNamingHelpers QueryNamingHelpers { get; }

        /// <summary>
        /// The optimizers
        /// </summary>
        QueryPostProcesses QueryPostProcesses { get; }

        /// <summary>
        /// Gets the schema provider.
        /// </summary>
        /// <value>The schema provider.</value>
        IDbSchemaProvider SchemaProvider { get; }

        /// <summary>
        /// The type cache
        /// </summary>
        ITypeCache TypeCache { get; }

        /// <summary>
        /// Creates a sparql variable.
        /// </summary>
        /// <returns>The variable name.</returns>
        string CreateSparqlVariable();

        /// <summary>
        /// Gets the blank node for value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        INode GetBlankNodeForValue(INodeFactory factory, object value);
    }
}