namespace Slp.Evi.Storage.Core

open System
open VDS.RDF
open VDS.RDF.Parsing
open Slp.Evi
open TCode.r2rml4net
open Slp.Evi.Relational
open Slp.Evi.Database

type QueryProcessor<'T> private (bgpMappings: Sparql.Algebra.BasicGraphPatternMapping list, database: ISqlDatabase<'T>) =
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
            |> DatabaseQueryBuilder.translateToQuery database.DatabaseSchema

        use queryResult = databaseQuery |> database.Writer.WriteQuery |> database.ExecuteQuery

        match query.QueryType with
        | Query.SparqlQueryType.Ask ->
            resultsHandler.StartResults()
            try
                if queryResult.HasNextRow |> not then
                    "Expected a row from SQL query execution" |> invalidOp
                else
                    let row = queryResult.ReadRow()

                    match row.Columns |> Seq.tryExactlyOne with
                    | Some(column) ->
                        //let booleanValue = row.GetColumn(column).GetBooleanValue()
                        //resultsHandler.HandleBooleanResult(booleanValue);
                        "ASK not yet implemented" |> invalidOp

                    | None ->
                        "Expected exactly one column in the SQL result" |> invalidOp

                    if queryResult.HasNextRow then
                        "Expected only a single row from SQL query execution" |> invalidOp

                resultsHandler.EndResults(true)
            with
            | _ ->
                resultsHandler.EndResults(false)
                reraise()

            "ASK query type is not yet implemented" |> NotImplementedException |> raise

        | Query.SparqlQueryType.Construct ->
            rdfHandler.StartRdf()

            try
                let template = query.ConstructTemplate

                let rec processConstructTemplate (curResult: ISqlResultReader) =
                    if curResult.HasNextRow then
                        let row = curResult.ReadRow()

                        let s = new VDS.RDF.Query.Algebra.Set()

                        sqlAlgebra.Bindings
                        |> Map.iter (
                            fun variable binder ->
                                let maybeNode = ValueBinderLoader.loadValue rdfHandler databaseQuery.NamingProvider row binder

                                match maybeNode with
                                | Some node ->
                                    s.Add(variable |> ValueBinderLoader.getVariableName, node)
                                | None ->
                                    ()
                        )

                        let ctx = new VDS.RDF.Query.Construct.ConstructContext(rdfHandler, s, false)

                        template.TriplePatterns
                        |> Seq.choose (
                            function
                            | :? VDS.RDF.Query.Patterns.IConstructTriplePattern as p ->
                                Some p
                            | x ->
                                sprintf "Unsupported triple pattern in construct template: %s" (x.GetType().AssemblyQualifiedName)
                                |> invalidOp
                        )
                        |> Seq.iter (
                            fun triplePattern ->
                                try
                                    if ctx |> triplePattern.Construct |> rdfHandler.HandleTriple |> not then
                                        "RDF Handler failed to handle a triple" |> invalidOp
                                with
                                | :? VDS.RDF.Query.RdfQueryException ->
                                    // If we get an error here then we could not construct a specific triple
                                    // so we continue anyway
                                    ()
                        )

                        processConstructTemplate curResult

                processConstructTemplate queryResult

                rdfHandler.EndRdf(true)

            with
            | _ ->
                rdfHandler.EndRdf(false)
                reraise()

        | Query.SparqlQueryType.Describe
        | Query.SparqlQueryType.DescribeAll ->
            let (sBinder, pBinder, oBinder) =
                match sqlAlgebra.Variables with
                | [ s; p; o ] ->
                    sqlAlgebra.Bindings.[s], sqlAlgebra.Bindings.[p], sqlAlgebra.Bindings.[o]
                | _ ->
                    "Expected 3 variables in DESCRIBE query"
                    |> invalidOp

            rdfHandler.StartRdf()

            try
                let rec processResult (curResult: ISqlResultReader) =
                    if curResult.HasNextRow then
                        let row = curResult.ReadRow()

                        let sMaybeNode = sBinder |> ValueBinderLoader.loadValue rdfHandler databaseQuery.NamingProvider row
                        let pMaybeNode = pBinder |> ValueBinderLoader.loadValue rdfHandler databaseQuery.NamingProvider row
                        let oMaybeNode = oBinder |> ValueBinderLoader.loadValue rdfHandler databaseQuery.NamingProvider row

                        match sMaybeNode, pMaybeNode, oMaybeNode with
                        | Some(sNode), Some(pNode), Some(oNode) ->
                            if rdfHandler.HandleTriple(new VDS.RDF.Triple(sNode, pNode, oNode)) |> not then
                                "RDF Handler failed to handle a triple" |> invalidOp
                        | _ ->
                            ()

                        processResult curResult

                processResult queryResult

                rdfHandler.EndRdf(true)
            with
            | _ ->
                rdfHandler.EndRdf(false)
                reraise()

        | Query.SparqlQueryType.Select
        | Query.SparqlQueryType.SelectAll
        | Query.SparqlQueryType.SelectAllDistinct
        | Query.SparqlQueryType.SelectAllReduced
        | Query.SparqlQueryType.SelectDistinct
        | Query.SparqlQueryType.SelectReduced ->
            resultsHandler.StartResults()

            try
                sqlAlgebra.Variables
                |> List.iter (
                    fun var ->
                        if resultsHandler.HandleVariable(var |> ValueBinderLoader.getVariableName) |> not then
                            "Results handler failed to handle a variable" |> invalidOp
                )

                let rec processResult (curResult: ISqlResultReader) =
                    if curResult.HasNextRow then
                        let row = curResult.ReadRow()

                        let s = new VDS.RDF.Query.Algebra.Set()

                        sqlAlgebra.Bindings
                        |> Map.iter (
                            fun variable binder ->
                                let maybeNode = ValueBinderLoader.loadValue resultsHandler databaseQuery.NamingProvider row binder

                                match maybeNode with
                                | Some node ->
                                    s.Add(variable |> ValueBinderLoader.getVariableName, node)
                                | None ->
                                    ()
                        )

                        if resultsHandler.HandleResult(new VDS.RDF.Query.SparqlResult(s)) |> not then
                            "Results handler failed to handle result" |> invalidOp

                processResult queryResult

                resultsHandler.EndResults(true)
            with
            | _ ->
                resultsHandler.EndResults(false)
                reraise()

        | _ ->
            "Unknown query type cannot be processed" |> invalidOp

    new (mapping: IR2RML, database: ISqlDatabase<'T>) =
        let processedMapping = R2RML.Builder.createMappingRepresentation database.DatabaseSchema mapping
        let bgpMappings = Sparql.R2RMLMappingProcessor.generateBasicGraphPatternMapping processedMapping
        
        QueryProcessor(bgpMappings, database)

    member _.Query(rdfHandler: IRdfHandler, resultsHandler: ISparqlResultsHandler, sparqlQuery: string): unit =
        let parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1)
        
        sparqlQuery
        |> parser.ParseFromString
        |> performQuery (rdfHandler, resultsHandler)