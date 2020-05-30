namespace Slp.Evi.FuncStorage

open System
open VDS.RDF.Storage
open VDS.RDF
open VDS.RDF.Query
open VDS.RDF.Parsing.Handlers

type EviStorage(queryProcessor: QueryProcessor) =
    let query (queryString: string): obj =
        let graph = new Graph()
        let resultSet = new SparqlResultSet()
        queryProcessor.Query(new GraphHandler(graph), new ResultSetHandler(resultSet), queryString)
        if resultSet.ResultsType <> SparqlResultsType.Unknown then
            resultSet :> obj
        else
            graph :> obj
    
    let loadGraphUsingHandler (handler: IRdfHandler) (graphUri: Uri) =
        if graphUri = null then
            raise (NotSupportedException("Graph IRI cannot be null"))

        let queryString = SparqlParameterizedString()
        queryString.CommandText <- "CONSTRUCT { ?s ?p ?o } FROM @graph WHERE { ?s ?p ?o }"
        queryString.SetUri("graph", graphUri)
        ()

    let loadGraphUsingGraph (g: IGraph) (graphUri:Uri) =
        if g.IsEmpty && graphUri <> null then
            g.BaseUri <- graphUri
        loadGraphUsingHandler (new GraphHandler(g)) graphUri

    let transformGraphUri (uri: string) =
        if String.IsNullOrEmpty(uri) then
            null
        else
            UriFactory.Create(uri)

    interface IQueryableStorage with
        member this.DeleteGraph(graphUri: Uri): unit = 
            raise (NotSupportedException("Graph delete not supported"))

        member this.DeleteGraph(graphUri: string): unit = 
            raise (NotSupportedException("Graph delete not supported"))
        member this.DeleteSupported: bool = false

        member this.IOBehaviour: IOBehaviour = 
            raise (NotImplementedException("IOBehaviour listing not yet implemented"))

        member this.IsReadOnly: bool = true

        member this.IsReady: bool = 
            raise (NotImplementedException("IsReady check is not yet implemented"))

        member this.ListGraphs(): Collections.Generic.IEnumerable<Uri> = 
            match (query "SELECT DISTINCT ?graph WHERE { GRAPH ?graph {}}") with
            | :? SparqlResultSet as rset ->
                rset
                |> Seq.map (fun r ->
                    match r.TryGetValue "?graph" with
                    | true, x ->
                        match x with
                        | :? UriNode as uriNode ->
                            uriNode.Uri
                        | _ ->
                            raise (InvalidOperationException("Did not get uri in result set as expected"))
                    | _ -> raise (InvalidOperationException("Did not get the variable in result set as expected"))
                )
            | _ -> raise (InvalidOperationException("Did not get a SPARQL Result Set as expected"))

        member this.ListGraphsSupported: bool = true

        member this.LoadGraph(g: VDS.RDF.IGraph, graphUri: Uri): unit = 
            graphUri |> loadGraphUsingGraph g

        member this.LoadGraph(g: VDS.RDF.IGraph, graphUri: string): unit = 
            graphUri |> transformGraphUri |> loadGraphUsingGraph g

        member this.LoadGraph(handler: VDS.RDF.IRdfHandler, graphUri: Uri): unit = 
            graphUri |> loadGraphUsingHandler handler

        member this.LoadGraph(handler: VDS.RDF.IRdfHandler, graphUri: string): unit = 
            graphUri |> transformGraphUri |> loadGraphUsingHandler handler

        member this.ParentServer: Management.IStorageServer = null

        member this.Query(rdfHandler: VDS.RDF.IRdfHandler, resultsHandler: VDS.RDF.ISparqlResultsHandler, sparqlQuery: string): unit = 
            queryProcessor.Query(rdfHandler, resultsHandler, sparqlQuery)

        member this.SaveGraph(g: VDS.RDF.IGraph): unit = 
            raise (NotSupportedException("Graph save is not supported"))

        member this.UpdateGraph(graphUri: Uri, additions: Collections.Generic.IEnumerable<VDS.RDF.Triple>, removals: Collections.Generic.IEnumerable<VDS.RDF.Triple>): unit = 
            raise (NotSupportedException("Graph update is not supported"))

        member this.UpdateGraph(graphUri: string, additions: Collections.Generic.IEnumerable<VDS.RDF.Triple>, removals: Collections.Generic.IEnumerable<VDS.RDF.Triple>): unit = 
            raise (NotSupportedException("Graph update is not supported"))

        member this.UpdateSupported: bool = 
            false

        member this.Query(sparqlQuery: string): obj =
            query sparqlQuery

        member this.Dispose(): unit = ()

