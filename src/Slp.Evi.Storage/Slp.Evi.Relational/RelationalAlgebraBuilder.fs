namespace Slp.Evi.Relational

open Slp.Evi.Common
open Slp.Evi.R2RML
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Relational.Algebra
open Slp.Evi.Relational.ValueBinders
open System

module RelationalAlgebraBuilder =
    type TransformedSparqlPattern = CalculusModel * ValueBinding list

type RelationalAlgebraBuilder () =
    let processRestrictedTriplePattern (patterns: RestrictedPatternMatch list) =
        let findIds (subjectMap: IriMapping) =
            match subjectMap.Value with
            | IriColumn col -> col.Name |> Set.singleton
            | IriConstant _ -> Set.empty
            | IriTemplate template ->
                (Set.empty, template)
                ||> List.fold (
                    fun cur part ->
                        match part with
                        | MappingTemplate.TemplatePart.ColumnPart c -> cur |> Set.add c.Name
                        | _ -> cur
                )

        let findSource (sqlSources: Map<Pattern, SqlSource list>) idColumns pattern source =
            match source with
            | Table schema ->
                let mayBeExistingSource =
                    match sqlSources.TryGetValue pattern with
                    | true, variableSources ->
                        variableSources
                        |> List.tryFind (fun x -> x.Schema = schema)
                    | false, _ ->
                        None

                let mayBeUsableSource =
                    match mayBeExistingSource with
                    | Some existingSource ->
                        existingSource.Schema.Keys
                        |> Seq.map (Set.ofSeq)
                        |> Seq.tryFind (
                            fun keys ->
                                keys
                                |> Set.ofSeq
                                |> Set.isSubset idColumns
                        )
                        |> Option.map (fun _ -> existingSource)
                    | None ->
                        None
                
                mayBeUsableSource
                |> Option.defaultWith (fun () ->
                    {
                        Schema = schema
                        Columns =
                            schema.Columns
                            |> Seq.toList
                            |> List.map (fun col -> { Schema = schema.GetColumn(col) })
                    }
                )

            | Statement _ -> "SQL Statements are not yet supported" |> NotImplementedException |> raise

        let rec implPatternList sqlSources filters valueBindings toProcess =
            match toProcess with
            | [] ->
                {
                    Sources =
                        sqlSources
                        |> Map.toList
                        |> List.collect snd
                        |> List.map Sql
                    Assignments = []
                    Filters = filters
                }, valueBindings
            | current :: xs ->
                implPatternSubject sqlSources filters valueBindings current xs
        and implPatternSubject sqlSources filters valueBindings current toProcess =
            let idColumns = findIds current.Restriction.Subject.Value
            let source = findSource sqlSources idColumns current.PatternMatch.Subject current.Restriction.TriplesMap.Source

            // TODO: Continue here

            sprintf "Ended with source %A for table %A" source current
            |> invalidOp

        implPatternList Map.empty [] [] patterns

    let processSparqlPattern (sparqlPattern: SparqlPattern) =
        match sparqlPattern with
        | EmptyPattern ->
            { Sources = [ SingleEmptyResult ]; Assignments = []; Filters = [] }, []
        | NotMatchingPattern ->
            { Sources = [ NoResult ]; Assignments = []; Filters = [] }, []
        | NotProcessedTriplePatterns _ ->
            "Encountered NotProcessedTriplePatterns in RelationalAlgebraBuilder"
            |> invalidArg "sparqlPattern"
        | RestrictedTriplePatterns restrictedPatterns ->
            restrictedPatterns
            |> processRestrictedTriplePattern
        | _ ->
            sprintf "Ended with %A" sparqlPattern
            |> invalidOp

    let applyModifiers (modifiers: Modifier list) (inner: RelationalAlgebraBuilder.TransformedSparqlPattern) =
        sprintf "Ended with %A with modifiers to add %A" inner modifiers
        |> invalidOp

    member _.buildRelationalQuery (sparqlAlgebra: SparqlQuery) =
        sparqlAlgebra.Query
        |> processSparqlPattern
        |> applyModifiers sparqlAlgebra.Modifiers





