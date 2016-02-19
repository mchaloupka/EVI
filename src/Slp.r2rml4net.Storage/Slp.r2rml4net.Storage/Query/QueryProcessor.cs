using System;
using System.Linq;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Relational.Builder;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Builder;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Query
{
    /// <summary>
    /// Query processor
    /// </summary>
    public class QueryProcessor
    {
        /// <summary>
        /// The mapping processor
        /// </summary>
        private readonly MappingProcessor _mapping;

        /// <summary>
        /// The database
        /// </summary>
        private readonly ISqlDatabase _db;

        /// <summary>
        /// The sparql algebra builder
        /// </summary>
        private readonly SparqlBuilder _sparqlBuilder;

        /// <summary>
        /// The factory used to generate classes
        /// </summary>
        private readonly IR2RMLStorageFactory _factory;

        /// <summary>
        /// The database schema provider
        /// </summary>
        private readonly IDbSchemaProvider _schemaProvider;

        /// <summary>
        /// The relational builder
        /// </summary>
        private RelationalBuilder _relationalBuilder;


        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProcessor" /> class.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="factory">The factory.</param>
        public QueryProcessor(ISqlDatabase db, IR2RML mapping, IR2RMLStorageFactory factory)
        {
            _db = db;
            _factory = factory;
            _schemaProvider = new DbSchemaProvider(db);
            
            _mapping = factory.CreateMappingProcessor(mapping);
            _sparqlBuilder = factory.CreateSparqlBuilder();
            _relationalBuilder = factory.CreateRelationalBuilder();
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="rdfHandler">The RDF handler.</param>
        /// <param name="resultsHandler">The results handler.</param>
        /// <param name="sparqlQuery">The sparql query.</param>
        /// <exception cref="System.ArgumentNullException">
        /// resultsHandler;Cannot handle a Ask query with a null SPARQL Results Handler
        /// or
        /// rdfHandler;Cannot handle a Graph result with a null RDF Handler
        /// or
        /// rdfHandler;Cannot handle a Graph result with a null RDF Handler
        /// or
        /// resultsHandler;Cannot handle SPARQL Results with a null Results Handler
        /// </exception>
        /// <exception cref="System.Exception">Unable to process the results of an Unknown query type</exception>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            var originalQuery = parser.ParseFromString(sparqlQuery);

            INodeFactory nodeFactory;
            switch (originalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    if (resultsHandler == null)
                        throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle a Ask query with a null SPARQL Results Handler");

                    nodeFactory = resultsHandler;
                    break;
                case SparqlQueryType.Construct:
                    if (rdfHandler == null)
                        throw new ArgumentNullException(nameof(rdfHandler), "Cannot handle a Graph result with a null RDF Handler");

                    nodeFactory = rdfHandler;
                    break;
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (rdfHandler == null)
                        throw new ArgumentNullException(nameof(rdfHandler), "Cannot handle a Graph result with a null RDF Handler");

                    nodeFactory = rdfHandler;
                    break;
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (resultsHandler == null)
                        throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL Results with a null Results Handler");

                    nodeFactory = resultsHandler;
                    break;
                default:
                    throw new Exception("Unable to process the results of an Unknown query type");
            }

            // Convert to algebra
            var context = _factory.CreateQueryContext(originalQuery, _mapping, _db, _schemaProvider, nodeFactory);

            // Generate SQL algebra
            var sqlAlgebra = GenerateSqlAlgebra(context);

            // Create query
            var query = _db.GenerateQuery(sqlAlgebra, context);

            // Execute query
            using (var result = _db.ExecuteQuery(query))
            {
                ProcessResult(rdfHandler, resultsHandler, originalQuery, context, sqlAlgebra, result);
            }
            
            // TODO: Check this out:
            //if(sqlAlgebra is NoRowSource)
            //{
            //    using(var result = new StaticDataReader())
            //    {
            //        ProcessResult(rdfHandler, resultsHandler, originalQuery, context, sqlAlgebra, result);
            //    }
            //}
            //else if(sqlAlgebra is SingleEmptyRowSource)
            //{
            //    using (var result = new StaticDataReader(new StaticDataReaderRow()))
            //    {
            //        ProcessResult(rdfHandler, resultsHandler, originalQuery, context, sqlAlgebra, result);
            //    }
            //}
            //else
            //{
            //    // Query
            //    var query = _db.GenerateQuery(sqlAlgebra, context);

            //    // Execute query
            //    using (var result = _db.ExecuteQuery(query))
            //    {
            //        ProcessResult(rdfHandler, resultsHandler, originalQuery, context, sqlAlgebra, result);
            //    }
            //}
        }

        /// <summary>
        /// Processes the query result.
        /// </summary>
        /// <param name="rdfHandler">The RDF handler.</param>
        /// <param name="resultsHandler">The results handler.</param>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="context">The query context.</param>
        /// <param name="sqlAlgebra">The SQL algebra.</param>
        /// <param name="result">The SQL execution result.</param>
        /// <exception cref="System.Exception">
        /// Expected a column from sql query execution
        /// or
        /// Expected a single column from sql query execution
        /// or
        /// Expected a row from sql query execution
        /// or
        /// Expected 3 value binders in construct or describe query
        /// or
        /// Unable to process the results of an Unknown query type
        /// </exception>
        private static void ProcessResult(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery originalQuery, QueryContext context, RelationalQuery sqlAlgebra, IQueryResultReader result)
        {
            switch (originalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    resultsHandler.StartResults();
                    try
                    {
                        if (result.HasNextRow)
                        {
                            var row = result.Read();

                            if (!row.Columns.Any())
                            {
                                throw new Exception("Expected a column from sql query execution");
                            }
                            else if (row.Columns.Count() > 1)
                            {
                                throw new Exception("Expected a single column from sql query execution");
                            }

                            var boolValue = row.Columns.First().GetBooleanValue();

                            resultsHandler.HandleBooleanResult(boolValue);
                        }
                        else
                        {
                            throw new Exception("Expected a row from sql query execution");
                        }

                        resultsHandler.EndResults(true);
                    }
                    catch
                    {
                        resultsHandler.EndResults(false);
                        throw;
                    }


                    break;
                case SparqlQueryType.Construct:
                    rdfHandler.StartRdf();

                    try
                    {
                        var template = context.OriginalQuery.ConstructTemplate;

                        while (result.HasNextRow)
                        {
                            var row = result.Read();

                            ProcessConstructTemplate(rdfHandler, row, template, sqlAlgebra, context);
                        }

                        rdfHandler.EndRdf(true);
                    }
                    catch
                    {
                        rdfHandler.EndRdf(false);
                        throw;
                    }

                    break;
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (sqlAlgebra.ValueBinders.Count() != 3)
                        throw new Exception("Expected 3 value binders in construct or describe query");

                    var sBinder = sqlAlgebra.ValueBinders.ElementAt(0);
                    var pBinder = sqlAlgebra.ValueBinders.ElementAt(1);
                    var oBinder = sqlAlgebra.ValueBinders.ElementAt(2);

                    rdfHandler.StartRdf();
                    try
                    {
                        while (result.HasNextRow)
                        {
                            var row = result.Read();

                            var sNode = sBinder.LoadNode(rdfHandler, row, context);
                            var pNode = pBinder.LoadNode(rdfHandler, row, context);
                            var oNode = oBinder.LoadNode(rdfHandler, row, context);

                            if (sNode == null || pNode == null || oNode == null)
                                continue;

                            if (!rdfHandler.HandleTriple(new Triple(sNode, pNode, oNode))) break;
                        }

                        rdfHandler.EndRdf(true);
                    }
                    catch
                    {
                        rdfHandler.EndRdf(false);
                        throw;
                    }

                    break;
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    resultsHandler.StartResults();

                    try
                    {
                        foreach (var binder in sqlAlgebra.ValueBinders)
                        {
                            if (!resultsHandler.HandleVariable(binder.VariableName)) ParserHelper.Stop();
                        }

                        while (result.HasNextRow)
                        {
                            var row = result.Read();

                            var s = new Set();

                            foreach (var binder in sqlAlgebra.ValueBinders)
                            {
                                var val = binder.LoadNode(resultsHandler, row, context);

                                if (val != null)
                                    s.Add(binder.VariableName, val);
                            }

                            if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                        }

                        resultsHandler.EndResults(true);
                    }
                    catch
                    {
                        resultsHandler.EndResults(false);
                        throw;
                    }

                    break;
                case SparqlQueryType.Unknown:
                    throw new Exception("Unable to process the results of an Unknown query type");
                default:
                    break;
            }
        }

        /// <summary>
        /// Processes the construct template.
        /// </summary>
        /// <param name="rdfHandler">The RDF handler.</param>
        /// <param name="row">The database row.</param>
        /// <param name="template">The template.</param>
        /// <param name="sqlAlgebra">The SQL algebra.</param>
        /// <param name="context">The query context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private static void ProcessConstructTemplate(IRdfHandler rdfHandler, IQueryResultRow row, GraphPattern template, RelationalQuery sqlAlgebra, QueryContext context)
        {
            var s = new Set();

            foreach (var binder in sqlAlgebra.ValueBinders)
            {
                var val = binder.LoadNode(rdfHandler, row, context);

                if (val != null)
                    s.Add(binder.VariableName, val);
            }

            var constructContext = new ConstructContext(rdfHandler, s, false);

            // NOTE: Currently we support only simple triples
            foreach (var pattern in template.TriplePatterns)
            {
                if (pattern is TriplePattern)
                {
                    var triplePattern = (TriplePattern)pattern;
                    var triple = triplePattern.Construct(constructContext);
                    rdfHandler.HandleTriple(triple);
                }
                else
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Generates the SQL algebra.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SQL algebra.</returns>
        private RelationalQuery GenerateSqlAlgebra(QueryContext context)
        {
            var algebra = _sparqlBuilder.Process(context);

            // TODO: Transform graph and from statements

            // TODO: Convert to safe form

            var relationalAlgebra = _relationalBuilder.Process(algebra, context);

            //// Transform to SQL algebra
            //var sqlAlgebra = _sqlAlgebraBuilder.Process(algebra, context);

            //// Optimize sql algebra
            //foreach (var optimizer in _sqlOptimizers)
            //{
            //    sqlAlgebra = optimizer.ProcessAlgebra(sqlAlgebra, context);
            //}

            //return sqlAlgebra;

            return relationalAlgebra;
        }
    }
}
