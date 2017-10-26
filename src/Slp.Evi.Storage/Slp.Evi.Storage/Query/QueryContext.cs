using System.Collections.Generic;
using Slp.Evi.Storage.Bootstrap;
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
    public class QueryContext : IQueryContext
    {
        /// <summary>
        /// The blank nodes.
        /// </summary>
        private readonly Dictionary<string, INode> _blankNodes;

        /// <summary>
        /// The used variables
        /// </summary>
        private readonly HashSet<string> _usedVariables;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext" /> class.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="schemaProvider">The schema provider.</param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="factory">The storage factory</param>
        /// <param name="typeCache"></param>
        public QueryContext(SparqlQuery originalQuery, IMappingProcessor mapping, ISqlDatabase db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory, IEviQueryableStorageFactory factory, ITypeCache typeCache)
        {
            TypeCache = typeCache;
            OriginalQuery = originalQuery;
            OriginalAlgebra = originalQuery.ToAlgebra();
            NodeFactory = nodeFactory;
            Db = db;
            Mapping = mapping;
            SchemaProvider = schemaProvider;
            _blankNodes = new Dictionary<string, INode>();
            _blankNodes = new Dictionary<string, INode>();
            _usedVariables = new HashSet<string>(OriginalAlgebra.Variables);
            QueryNamingHelpers = new QueryNamingHelpers(this);
            QueryPostProcesses = new QueryPostProcesses(factory, this);
        }

        /// <summary>
        /// Gets the schema provider.
        /// </summary>
        /// <value>The schema provider.</value>
        public IDbSchemaProvider SchemaProvider { get; private set; }

        /// <summary>
        /// Gets the original query.
        /// </summary>
        /// <value>The original query.</value>
        public SparqlQuery OriginalQuery { get; private set; }

        /// <summary>
        /// Gets the mapping.
        /// </summary>
        /// <value>The mapping.</value>
        public IMappingProcessor Mapping { get; private set; }

        /// <summary>
        /// Gets the node factory.
        /// </summary>
        /// <value>The node factory.</value>
        public INodeFactory NodeFactory { get; private set; }

        /// <summary>
        /// Gets the original algebra.
        /// </summary>
        /// <value>The original algebra.</value>
        public ISparqlAlgebra OriginalAlgebra { get; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        public ISqlDatabase Db { get; private set; }

        /// <summary>
        /// Gets the query naming helpers.
        /// </summary>
        /// <value>The query naming helpers.</value>
        public QueryNamingHelpers QueryNamingHelpers { get; }

        /// <summary>
        /// The optimizers
        /// </summary>
        public QueryPostProcesses QueryPostProcesses { get; }

        /// <summary>
        /// The type cache
        /// </summary>
        public ITypeCache TypeCache { get; }

        /// <summary>
        /// Gets the blank node subject for value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <returns>Node.</returns>
        public INode GetBlankNodeForValue(INodeFactory factory, object value)
        {
            var sVal = value.ToString();

            if (!_blankNodes.ContainsKey(sVal))
            {
                _blankNodes.Add(sVal, factory.CreateBlankNode());
            }

            return _blankNodes[sVal];
        }

        /// <summary>
        /// Creates the sparql variable.
        /// </summary>
        /// <returns>The variable name.</returns>
        public string CreateSparqlVariable()
        {
            int counter = 1;
            string varName;

            do
            {
                varName = $"_:context-autos{counter++}";
            } while (_usedVariables.Contains(varName));

            _usedVariables.Add(varName);
            return varName;
        }
    }
}
