using System;
using System.Collections.Generic;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Query;
using TCode.r2rml4net;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;

namespace Slp.r2rml4net.Storage
{
    /// <summary>
    /// The R2RML Storage
    /// </summary>
    public class R2RmlStorage : IQueryableStorage
    {
        /// <summary>
        /// The query processor
        /// </summary>
        private readonly QueryProcessor _queryProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="R2RmlStorage" /> class.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="factory">The factory.</param>
        public R2RmlStorage(ISqlDatabase db, IR2RML mapping, IR2RmlStorageFactory factory)
        {
            _queryProcessor = factory.CreateQueryProcessor(db, mapping);
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying store processing the resulting Graph/Result Set with a handler of your choice
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            _queryProcessor.Query(rdfHandler, resultsHandler, sparqlQuery);
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns><see cref="T:VDS.RDF.Query.SparqlResultSet">SparqlResultSet</see> or a <see cref="T:VDS.RDF.Graph">Graph</see> depending on the Sparql Query</returns>
        public object Query(string sparqlQuery)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }

            return g;
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to be deleted</param>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <remarks><strong>Note:</strong> Not all Stores are capable of Deleting a Graph so it is acceptable for such a Store to throw a <see cref="T:System.NotSupportedException">NotSupportedException</see> or an <see cref="T:VDS.RDF.Storage.RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality</remarks>
        public void DeleteGraph(string graphUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to be deleted</param>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <remarks><strong>Note:</strong> Not all Stores are capable of Deleting a Graph so it is acceptable for such a Store to throw a <see cref="T:System.NotSupportedException">NotSupportedException</see> or an <see cref="T:VDS.RDF.Storage.RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality</remarks>
        public void DeleteGraph(Uri graphUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a List of Graph URIs for the graphs in the store
        /// </summary>
        /// <returns>IEnumerable&lt;Uri&gt;.</returns>
        /// <exception cref="System.Exception">
        /// Did not get uri in result set as expected
        /// or
        /// Did not get the variable in result set as expected
        /// or
        /// Did not get a SPARQL Result Set as expected
        /// </exception>
        /// <remarks>Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.</remarks>
        public IEnumerable<Uri> ListGraphs()
        {
            var result = Query("SELECT DISTINCT ?graph WHERE { GRAPH ?graph {}}");

            if (result is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)result;

                foreach (SparqlResult r in rset)
                {
                    INode node;

                    if (r.TryGetValue("?graph", out node))
                    {
                        if (node is UriNode)
                        {
                            yield return ((UriNode)node).Uri;
                        }
                        else
                        {
                            throw new Exception("Did not get uri in result set as expected");
                        }
                    }
                    else
                    {
                        throw new Exception("Did not get the variable in result set as expected");
                    }
                }
            }
            else
            {
                throw new Exception("Did not get a SPARQL Result Set as expected");
            }
        }

        /// <summary>
        /// Loads a Graph from the Store using the RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.</remarks>
        public void LoadGraph(IRdfHandler handler, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                LoadGraph(handler, (Uri)null);
            }
            else
            {
                LoadGraph(handler, UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the Store using the RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <exception cref="System.ArgumentException">Graph uri cannot be null  or empty;graphUri</exception>
        /// <remarks>Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.</remarks>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            if (graphUri == null)
                throw new ArgumentException("Graph uri cannot be null  or empty", "graphUri");

            SparqlParameterizedString queryString = new SparqlParameterizedString
            {
                CommandText = "CONSTRUCT { ?s ?p ?o } FROM @graph WHERE { ?s ?p ?o }"
            };
            queryString.SetUri("graph", graphUri);

            Query(handler, new ResultSetHandler(new SparqlResultSet()), queryString.ToString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks><para>
        /// If the Graph being loaded into is Empty then it's Base Uri should become the Uri of the Graph being loaded, otherwise it should be merged into the existing non-empty Graph whose Base Uri should be unaffected.
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para></remarks>
        public void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                LoadGraph(g, (Uri)null);
            }
            else
            {
                LoadGraph(g, UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks><para>
        /// If the Graph being loaded into is Empty then it's Base Uri should become the Uri of the Graph being loaded, otherwise it should be merged into the existing non-empty Graph whose Base Uri should be unaffected.
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para></remarks>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (g.IsEmpty && graphUri != null)
            {
                g.BaseUri = graphUri;
            }

            LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Gets the Parent Server on which this store is hosted (if any)
        /// </summary>
        /// <value>The parent server.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>For storage backends which support multiple stores this is useful because it provides a way to access all the stores on that backend.  For stores which are standalone they should simply return null</remarks>
        public IStorageServer ParentServer
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Saves a Graph to the Store
        /// </summary>
        /// <param name="g">Graph to Save</param>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <remarks>Uri of the Graph should be taken from the <see cref="P:VDS.RDF.IGraph.BaseUri">BaseUri</see> property
        /// <br /><br />
        /// Behaviour of this method with regards to whether it overwrites/updates/merges with existing Graphs of the same Uri is up to the implementor and may be dependent on the underlying store.  Implementors <strong>should</strong> state in the XML comments for their implementations what behaviour is implemented.</remarks>
        public void SaveGraph(IGraph g)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to add to the Graph</param>
        /// <param name="removals">Triples to remove from the Graph</param>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <remarks><para>
        ///   <strong>Note:</strong> Not all Stores are capable of supporting update at the individual Triple level and as such it is acceptable for such a Store to throw a <see cref="T:System.NotSupportedException">NotSupportedException</see> or an <see cref="T:VDS.RDF.Storage.RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graph is up to the implementor, it may create a new empty Graph and apply the updates to that or it may throw an error.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// <para>
        /// Implementers <strong>MUST</strong> allow for either the additions or removals argument to be null
        /// </para></remarks>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to add to the Graph</param>
        /// <param name="removals">Triples to remove from the Graph</param>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <remarks><para>
        ///   <strong>Note:</strong> Not all Stores are capable of supporting update at the individual Triple level and as such it is acceptable for such a Store to throw a <see cref="T:System.NotSupportedException">NotSupportedException</see> if the Store cannot provide this functionality
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graph is up to the implementor, it may create a new empty Graph and apply the updates to that or it may throw an error.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// <para>
        /// Implementers <strong>MUST</strong> allow for either the additions or removals argument to be null
        /// </para></remarks>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets whether the deletion of graphs is supported
        /// </summary>
        /// <value><c>true</c> if [delete supported]; otherwise, <c>false</c>.</value>
        /// <remarks>Some Stores do not support the deletion of Graphs and may as designated in the interface definition throw a <see cref="T:System.NotSupportedException">NotSupportedException</see> if the <strong>DeleteGraph()</strong> method is called.  This property allows for calling code to check in advance whether Deletion of Graphs is supported.</remarks>
        public bool DeleteSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the Save Behaviour the Store uses
        /// </summary>
        /// <value>The io behaviour.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public IOBehaviour IOBehaviour
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets whether the connection with the underlying Store is read-only
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        /// <remarks>Any Manager which indicates it is read-only should also return false for the <see cref="P:VDS.RDF.Storage.IStorageCapabilities.UpdateSupported">UpdatedSupported</see> property and should throw a <see cref="T:VDS.RDF.Storage.RdfStorageException">RdfStorageException</see> if the <strong>SaveGraph()</strong> or <strong>UpdateGraph()</strong> methods are called</remarks>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether the connection with the underlying Store is ready for use
        /// </summary>
        /// <value><c>true</c> if this instance is ready; otherwise, <c>false</c>.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsReady
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets whether the Store supports Listing Graphs
        /// </summary>
        /// <value><c>true</c> if [list graphs supported]; otherwise, <c>false</c>.</value>
        public bool ListGraphsSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether the triple level updates are supported
        /// </summary>
        /// <value><c>true</c> if [update supported]; otherwise, <c>false</c>.</value>
        /// <remarks>Some Stores do not support updates at the Triple level and may as designated in the interface definition throw a <see cref="T:System.NotSupportedException">NotSupportedException</see> if the <strong>UpdateGraph()</strong> method is called.  This property allows for calling code to check in advance whether Updates are supported</remarks>
        public bool UpdateSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}
