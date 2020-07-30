namespace Slp.Evi.Storage.Core

open System
open VDS.RDF
open VDS.RDF.Parsing
open Slp.Evi
open TCode.r2rml4net
open Slp.Evi.Relational
open Slp.Evi.Database

type QueryProcessor<'T, 'C> private (bgpMappings: Sparql.Algebra.BasicGraphPatternMapping list, database: ISqlDatabase<'T, 'C>) =
    let mappingProcessor = Sparql.R2RMLMappingProcessor(bgpMappings)

    let generateSqlAlgebra (query: Query.SparqlQuery) =
        let typeIndexer = TypeIndexer()

        query
        |> Sparql.SparqlQueryBuilder.buildSparqlQuery
        |> mappingProcessor.processSparqlQuery
        |> RelationalAlgebraBuilder.buildRelationalQuery database.DatabaseSchema typeIndexer

    let performQuery (rdfHandler: IRdfHandler, resultsHandler: ISparqlResultsHandler) (query: Query.SparqlQuery) =
        let sqlAlgebra = generateSqlAlgebra query
        let databaseQuery =
            sqlAlgebra
            |> DatabaseQueryBuilder.translateToQuery

        raise (NotImplementedException(sprintf "Has not been further implemented, last part ended up with %A" databaseQuery))

    new (mapping: IR2RML, database: ISqlDatabase<'T, 'C>) =
        let processedMapping = R2RML.Builder.createMappingRepresentation database.DatabaseSchema mapping
        let bgpMappings = Sparql.R2RMLMappingProcessor.generateBasicGraphPatternMapping processedMapping
        
        QueryProcessor(bgpMappings, database)

    member _.Query(rdfHandler: IRdfHandler, resultsHandler: ISparqlResultsHandler, sparqlQuery: string): unit =
        let parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1)
        
        sparqlQuery
        |> parser.ParseFromString
        |> performQuery (rdfHandler, resultsHandler)