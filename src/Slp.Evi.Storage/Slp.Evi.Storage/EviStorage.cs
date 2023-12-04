using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Core.Database;
using Slp.Evi.Storage.Core;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;

namespace Slp.Evi.Storage
{
    public abstract class EviStorage<TQuery>
        : IQueryableStorage
    {
        private readonly QueryProcessor<TQuery> _queryProcessor;

        protected EviStorage(IR2RML mapping, ISqlDatabase<TQuery> database)
        {
            _queryProcessor = new QueryProcessor<TQuery>(mapping, database);
        }

        /// <inheritdoc />
        public bool IsReady
            => throw new NotImplementedException("IsReady check is not yet implemented");

        /// <inheritdoc />
        public bool IsReadOnly => true;

        /// <inheritdoc />
        public IOBehaviour IOBehaviour
            => throw new NotImplementedException("IOBehaviour listing not yet implemented");

        /// <inheritdoc />
        public bool UpdateSupported => false;

        /// <inheritdoc />
        public bool DeleteSupported => false;

        /// <inheritdoc />
        public bool ListGraphsSupported => false;

        private Uri GetGraphUri(string graphUri)
            => string.IsNullOrEmpty(graphUri) ? null : new Uri(graphUri);

        /// <inheritdoc />
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (g.IsEmpty && graphUri != null)
            {
                g.BaseUri = graphUri;
            }

            LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <inheritdoc />
        public void LoadGraph(IGraph g, string graphUri)
            => LoadGraph(g, GetGraphUri(graphUri));

        /// <inheritdoc />
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            if (graphUri == null)
            {
                throw new ArgumentException("Graph IRI cannot be null", nameof(graphUri));
            }

            var queryString = new SparqlParameterizedString("CONSTRUCT { ?s ?p ?o } FROM @graph WHERE { ?s ?p ?o }");
            queryString.SetUri("graph", graphUri);

            Query(handler, null, queryString.ToString());
        }

        /// <inheritdoc />
        public void LoadGraph(IRdfHandler handler, string graphUri)
            => LoadGraph(handler, GetGraphUri(graphUri));

        /// <inheritdoc />
        public void SaveGraph(IGraph g)
            => throw new NotSupportedException("Save graph operation is not supported");

        /// <inheritdoc />
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
            => throw new NotSupportedException("Update graph is not supported");

        /// <inheritdoc />
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
            => throw new NotSupportedException("Update graph is not supported");

        /// <inheritdoc />
        public void DeleteGraph(Uri graphUri)
            => throw new NotSupportedException("Delete graph is not supported");

        /// <inheritdoc />
        public void DeleteGraph(string graphUri)
            => throw new NotSupportedException("Delete graph is not supported");

        /// <inheritdoc />
        public IEnumerable<Uri> ListGraphs()
        {
            if (Query("SELECT DISTINCT ?graph WHERE { GRAPH ?graph {}}") is SparqlResultSet result)
            {
                return result.Select(row =>
                {
                    if (row.TryGetValue("?graph", out var graphNode))
                    {
                        if (graphNode is IUriNode uriNode)
                        {
                            return uriNode.Uri;
                        }
                        else
                        {
                            throw new InvalidOperationException("Did not get uri in result set as expected");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Did not get the variable in result set as expected");
                    }
                });
            }
            else
            {
                throw new InvalidOperationException("Did not get a SPARQL Result Set as expected");
            }
        }

        /// <inheritdoc />
        public IStorageServer ParentServer => null;

        /// <inheritdoc />
        public object Query(string sparqlQuery)
        {
            var graph = new Graph();
            var graphHandler = new GraphHandler(graph);

            var resultSet = new SparqlResultSet();
            var resultSetHandler = new ResultSetHandler(resultSet);

            Query(graphHandler, resultSetHandler, sparqlQuery);

            if (resultSet.ResultsType != SparqlResultsType.Unknown)
            {
                return resultSet;
            }
            else
            {
                return graph;
            }
        }

        /// <inheritdoc />
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            _queryProcessor.Query(rdfHandler, resultsHandler, sparqlQuery);
        }

        protected virtual void Dispose(bool disposing)
        { }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void UpdateGraph(IRefNode graphName, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
            => throw new NotSupportedException("Update graph is not supported");

        public IEnumerable<string> ListGraphNames()
            => throw new NotSupportedException("List graph names is not supported");
    }
}
