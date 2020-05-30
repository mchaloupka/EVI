namespace Slp.Evi.Storage.Core

open System
open VDS.RDF
open VDS.RDF.Parsing
open Slp.Evi
open TCode.r2rml4net

type QueryProcessor private (bgpMappings: Sparql.Algebra.BasicGraphPatternMapping list) =
    let generateSqlAlgebra (query: Query.SparqlQuery) =
        query
        |> Sparql.SparqlQueryBuilder.buildSparqlQuery
        |> Sparql.R2RMLMappingProcessor.processSparqlQuery bgpMappings

    let performQuery (rdfHandler: IRdfHandler, resultsHandler: ISparqlResultsHandler) (query: Query.SparqlQuery) =
        let sqlAlgebra = generateSqlAlgebra query

        raise (NotImplementedException(sprintf "Has not been further implemented, last part ended up with %A" sqlAlgebra))

    new (mapping: IR2RML) =
        let processedMapping = R2RML.Builder.createMappingRepresentation mapping
        let bgpMappings = Sparql.R2RMLMappingProcessor.generateBasicGraphPatternMapping processedMapping
        QueryProcessor(bgpMappings)

    member _.Query(rdfHandler: IRdfHandler, resultsHandler: ISparqlResultsHandler, sparqlQuery: string): unit =
        let parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1)
        
        sparqlQuery
        |> parser.ParseFromString
        |> performQuery (rdfHandler, resultsHandler)