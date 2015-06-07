using System;
using System.Collections.Generic;
using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Relational.Database;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace Slp.r2rml4net.Storage.Query
{
    /// <summary>
    /// The query context.
    /// </summary>
    public class QueryContext
    {
        /// <summary>
        /// The used SQL source names set.
        /// </summary>
        private readonly HashSet<string> _usedSqlSourceNames;

        /// <summary>
        /// The blank nodes subjects.
        /// </summary>
        private readonly Dictionary<string, INode> _blankNodesSubjects;

        /// <summary>
        /// The blank nodes objects.
        /// </summary>
        private readonly Dictionary<string, INode> _blankNodesObjects;

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
        public QueryContext(SparqlQuery originalQuery, MappingProcessor mapping, ISqlDatabase db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory)
        {
            OriginalQuery = originalQuery;
            OriginalAlgebra = originalQuery.ToAlgebra();
            NodeFactory = nodeFactory;
            Db = db;
            Mapping = mapping;
            SchemaProvider = schemaProvider;
            _usedSqlSourceNames = new HashSet<string>();
            _blankNodesSubjects = new Dictionary<string, INode>();
            _blankNodesObjects = new Dictionary<string, INode>();
            _usedVariables = new HashSet<string>(OriginalAlgebra.Variables);
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
        public MappingProcessor Mapping { get; private set; }

        /// <summary>
        /// Gets the node factory.
        /// </summary>
        /// <value>The node factory.</value>
        public INodeFactory NodeFactory { get; private set; }

        /// <summary>
        /// Gets the original algebra.
        /// </summary>
        /// <value>The original algebra.</value>
        public ISparqlAlgebra OriginalAlgebra { get; private set; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        public ISqlDatabase Db { get; private set; }

        /// <summary>
        /// Determines whether the specified name is already used SQL source name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the specified name is already used SQL source name; otherwise, <c>false</c>.</returns>
        public bool IsAlreadyUsedSqlSourceName(string name)
        {
            return _usedSqlSourceNames.Contains(name);
        }

        /// <summary>
        /// Registers the name of the used SQL source.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentException">This sql source name already used;name</exception>
        public void RegisterUsedSqlSourceName(string name)
        {
            if (_usedSqlSourceNames.Contains(name))
            {
                throw new ArgumentException("This sql source name already used", "name");
            }
            else
            {
                _usedSqlSourceNames.Add(name);
            }
        }

        /// <summary>
        /// Gets the blank node subject for value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <returns>Node.</returns>
        public INode GetBlankNodeSubjectForValue(INodeFactory factory, object value)
        {
            var sVal = value.ToString();

            if (!_blankNodesSubjects.ContainsKey(sVal))
            {
                _blankNodesSubjects.Add(sVal, factory.CreateBlankNode());
            }

            return _blankNodesSubjects[sVal];
        }

        /// <summary>
        /// Gets the blank node object for value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <returns>Node.</returns>
        public INode GetBlankNodeObjectForValue(INodeFactory factory, object value)
        {
            var sVal = value.ToString();

            if (!_blankNodesObjects.ContainsKey(sVal))
            {
                _blankNodesObjects.Add(sVal, factory.CreateBlankNode());
            }

            return _blankNodesObjects[sVal];
        }

        /// <summary>
        /// Optimizes the algebra on the fly.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <returns>The optimized algebra.</returns>
        public RelationalQuery OptimizeOnTheFly(RelationalQuery algebra)
        {
            var currentAlgebra = algebra;

            return currentAlgebra;
        }

        /// <summary>
        /// Optimizes the algebra on the fly.
        /// </summary>
        /// <param name="algebra">The SPARQL query.</param>
        /// <returns>The optimized SPARQL query.</returns>
        public ISparqlQuery OptimizeOnTheFly(ISparqlQuery algebra)
        {
            var currentAlgebra = algebra;

            return currentAlgebra;
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
                varName = string.Format("_:context-autos{0}", counter++);
            } while (_usedVariables.Contains(varName));

            _usedVariables.Add(varName);
            return varName;
        }
    }
}
