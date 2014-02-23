using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
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
    public class R2RMLStorage : IQueryableStorage
    {
        private ISqlDb db;
        private MappingProcessor mapping;

        public R2RMLStorage(IR2RML mapping, ISqlDb db)
        {
            this.mapping = new MappingProcessor(mapping);
            this.db = db;
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            var processor = new QueryProcessor(mapping, db);
            processor.Query(rdfHandler, resultsHandler, sparqlQuery);
        }

        public object Query(string sparqlQuery)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }

            return g;
        }

        public void DeleteGraph(string graphUri)
        {
            throw new NotSupportedException();
        }

        public void DeleteGraph(Uri graphUri)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<Uri> ListGraphs()
        {
            var result = this.Query("SELECT DISTINCT ?graph WHERE { GRAPH ?graph {}}");

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

        public void LoadGraph(IRdfHandler handler, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.LoadGraph(handler, (Uri)null);
            }
            else
            {
                this.LoadGraph(handler, UriFactory.Create(graphUri));
            }
        }

        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            if (graphUri == null)
                throw new ArgumentException("Graph uri cannot be null  or empty", "graphUri");

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.CommandText = "CONSTRUCT { ?s ?p ?o } FROM @graph WHERE { ?s ?p ?o }";
            queryString.SetUri("graph", graphUri);

            this.Query(handler, new ResultSetHandler(new SparqlResultSet()), queryString.ToString());
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.LoadGraph(g, (Uri)null);
            }
            else
            {
                this.LoadGraph(g, UriFactory.Create(graphUri));
            }
        }

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (g.IsEmpty && graphUri != null)
            {
                g.BaseUri = graphUri;
            }

            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        public IStorageServer ParentServer
        {
            get { throw new NotImplementedException(); }
        }

        public void SaveGraph(IGraph g)
        {
            throw new NotSupportedException();
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotSupportedException();
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotSupportedException();
        }

        public bool DeleteSupported
        {
            get { return false; }
        }

        public IOBehaviour IOBehaviour
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool IsReady
        {
            get { throw new NotImplementedException(); }
        }

        public bool ListGraphsSupported
        {
            get { return true; }
        }

        public bool UpdateSupported
        {
            get { return false; }
        }

        public void Dispose()
        {
            
        }
    }
}
