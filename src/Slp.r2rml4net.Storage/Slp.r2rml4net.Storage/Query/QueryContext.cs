using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Optimization;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
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
        private HashSet<string> usedSqlSourceNames;

        /// <summary>
        /// The blank nodes subjects.
        /// </summary>
        private Dictionary<string, INode> blankNodesSubjects;

        /// <summary>
        /// The blank nodes objects.
        /// </summary>
        private Dictionary<string, INode> blankNodesObjects;

        /// <summary>
        /// The used variables
        /// </summary>
        private HashSet<string> usedVariables;

        /// <summary>
        /// The sparql algebra optimizers on the fly
        /// </summary>
        private ISparqlAlgebraOptimizerOnTheFly[] sparqlAlgebraOptimizerOnTheFly;

        /// <summary>
        /// The SQL algebra optimizers on the fly
        /// </summary>
        private ISqlAlgebraOptimizerOnTheFly[] sqlAlgebraOptimizerOnTheFly;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext"/> class.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="sparqlAlgebraOptimizerOnTheFly">The sparql algebra optimizer on the fly.</param>
        /// <param name="sqlAlgebraOptimizerOnTheFly">The SQL algebra optimizer on the fly.</param>
        public QueryContext(SparqlQuery originalQuery, MappingProcessor mapping, ISqlDb db, INodeFactory nodeFactory, ISparqlAlgebraOptimizerOnTheFly[] sparqlAlgebraOptimizerOnTheFly, ISqlAlgebraOptimizerOnTheFly[] sqlAlgebraOptimizerOnTheFly)
        {
            this.OriginalQuery = originalQuery;
            this.OriginalAlgebra = originalQuery.ToAlgebra();
            this.NodeFactory = nodeFactory;
            this.Db = db;
            this.Mapping = mapping;
            this.usedSqlSourceNames = new HashSet<string>();
            this.blankNodesSubjects = new Dictionary<string, INode>();
            this.blankNodesObjects = new Dictionary<string, INode>();
            this.usedVariables = new HashSet<string>(this.OriginalAlgebra.Variables);
            this.sparqlAlgebraOptimizerOnTheFly = sparqlAlgebraOptimizerOnTheFly;
            this.sqlAlgebraOptimizerOnTheFly = sqlAlgebraOptimizerOnTheFly;
        }

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
        public ISqlDb Db { get; private set; }

        /// <summary>
        /// Determines whether the specified name is already used SQL source name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the specified name is already used SQL source name; otherwise, <c>false</c>.</returns>
        public bool IsAlreadyUsedSqlSourceName(string name)
        {
            return usedSqlSourceNames.Contains(name);
        }

        /// <summary>
        /// Registers the name of the used SQL source.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentException">This sql source name already used;name</exception>
        public void RegisterUsedSqlSourceName(string name)
        {
            if (usedSqlSourceNames.Contains(name))
            {
                throw new ArgumentException("This sql source name already used", "name");
            }
            else
            {
                usedSqlSourceNames.Add(name);
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

            if (!blankNodesSubjects.ContainsKey(sVal))
            {
                blankNodesSubjects.Add(sVal, factory.CreateBlankNode());
            }

            return blankNodesSubjects[sVal];
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

            if (!blankNodesObjects.ContainsKey(sVal))
            {
                blankNodesObjects.Add(sVal, factory.CreateBlankNode());
            }

            return blankNodesObjects[sVal];
        }

        /// <summary>
        /// Optimizes the algebra on the fly.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <returns>The optimized algebra.</returns>
        public INotSqlOriginalDbSource OptimizeOnTheFly(INotSqlOriginalDbSource algebra)
        {
            var currentAlgebra = algebra;

            foreach (var optimizer in sqlAlgebraOptimizerOnTheFly)
            {
                currentAlgebra = optimizer.ProcessAlgebraOnTheFly(currentAlgebra, this);
            }

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

            foreach (var optimizer in sparqlAlgebraOptimizerOnTheFly)
            {
                currentAlgebra = optimizer.ProcessAlgebraOnTheFly(currentAlgebra, this);
            }

            return currentAlgebra;
        }

        /// <summary>
        /// Creates the sparql variable.
        /// </summary>
        /// <returns>The variable name.</returns>
        public string CreateSparqlVariable()
        {
            int counter = 1;
            string varName = null;

            do
            {
                varName = string.Format("_:context-autos{0}", counter++);
            } while (this.usedVariables.Contains(varName));

            this.usedVariables.Add(varName);
            return varName;
        }
    }
}
