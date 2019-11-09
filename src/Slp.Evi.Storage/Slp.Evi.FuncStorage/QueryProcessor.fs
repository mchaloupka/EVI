namespace Slp.Evi.FuncStorage

open System
open VDS.RDF
open VDS.RDF.Parsing
open Slp.Evi.Sparql

type QueryProcessor() =
    let generateSqlAlgebra (query: Query.SparqlQuery) =
        let sparqlAlgebra = SparqlQueryBuilder.buildSparqlQuery query

        sparqlAlgebra

    let performQuery (rdfHandler: IRdfHandler, resultsHandler: ISparqlResultsHandler) (query: Query.SparqlQuery) =
        let sqlAlgebra = generateSqlAlgebra query

        raise (NotImplementedException(sprintf "Has not been further implemented, last part ended up with %A" sqlAlgebra))

    member this.Query(rdfHandler: IRdfHandler, resultsHandler: ISparqlResultsHandler, sparqlQuery: string): unit =
        let parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1)
        
        sparqlQuery
        |> parser.ParseFromString
        |> performQuery (rdfHandler, resultsHandler)