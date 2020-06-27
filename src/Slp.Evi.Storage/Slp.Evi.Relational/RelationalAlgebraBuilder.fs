namespace Slp.Evi.Relational

open Slp.Evi.Common
open Slp.Evi.R2RML
open Slp.Evi.Sparql.Algebra
open Slp.Evi.Relational.Algebra
open System

type RelationalAlgebraBuilder () =
    let valueBinderEqualToNodeCondition valueBinder node =
        sprintf "IsEqualValue for %A and %A" valueBinder node |> NotImplementedException |> raise

    let valueBindersEqualValueCondition valueBinder otherValueBinder =
        sprintf "IsEqualValue for %A and %A" valueBinder otherValueBinder |> NotImplementedException |> raise

    let isValueBinderBoundCondition valueBinder =
        sprintf "IsBoundCondition for %A" valueBinder |> NotImplementedException |> raise

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
                        |> List.tryFind (fun x -> x.Schema.Name = schema.Name)
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
            |> fun x ->
                let updatedSources = sqlSources |> Map.tryFind pattern |> Option.defaultValue List.empty |> fun xs -> x :: xs
                x, sqlSources |> Map.add pattern updatedSources

        let applyPatternMatch filters (valueBindings: Map<_,_>) source pattern mapping =
            let valueBinder =
                let neededVariables =
                    match mapping with
                    | IriObject iriObj ->
                        match iriObj.Value with
                        | IriColumn col -> col.Name |> List.singleton
                        | IriConstant _ -> List.empty
                        | IriTemplate tmpl ->
                            tmpl 
                            |> List.choose (
                                function
                                | MappingTemplate.ColumnPart col -> col.Name |> Some
                                | _ -> None
                            )
                    | LiteralObject litObj ->
                        match litObj.Value with
                        | LiteralColumn col -> col.Name |> List.singleton
                        | LiteralConstant _ -> List.empty
                        | LiteralTemplate tmpl ->
                            tmpl 
                            |> List.choose (
                                function
                                | MappingTemplate.ColumnPart col -> col.Name |> Some
                                | _ -> None
                            )

                let variables =
                    neededVariables
                    |> List.map (
                        fun var ->
                            var, source |>SqlSource.getColumn var |> Column
                    )
                    |> Map.ofList

                BaseValueBinder(mapping, variables)

            match pattern with
            | VariablePattern var ->
                match valueBindings.TryGetValue var with
                | true, otherValueBinder ->
                    isValueBinderBoundCondition valueBinder :: valueBindersEqualValueCondition valueBinder otherValueBinder :: filters, valueBindings
                | false, _ ->
                    isValueBinderBoundCondition valueBinder :: filters, valueBindings |> Map.add var valueBinder
            | NodeMatchPattern node ->
                isValueBinderBoundCondition valueBinder :: valueBinderEqualToNodeCondition valueBinder node :: filters, valueBindings

        let rec implPatternList sqlSources filters valueBindings toProcess =
            match toProcess with
            | [] ->
                {
                    Model = {
                        AlwaysBoundVariables =
                            sqlSources
                            |> Map.toList
                            |> List.collect snd
                            |> List.collect (fun x -> x.Columns)
                            |> List.map Column
                        OtherVariables = []
                        Sources =
                            sqlSources
                            |> Map.toList
                            |> List.collect snd
                            |> List.map Sql
                        Assignments = []
                        Filters = filters
                    }
                    Bindings = valueBindings
                }
            | current :: xs ->
                implPatternSubject sqlSources filters valueBindings current xs

        and implPatternSubject sqlSources filters valueBindings current toProcess =
            let idColumns = findIds current.Restriction.Subject.Value
            let (source, newSqlSources) = findSource sqlSources idColumns current.PatternMatch.Subject current.Restriction.TriplesMap.Source
            let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings source current.PatternMatch.Subject (current.Restriction.Subject.Value |> IriObject)
            implPatternPredicate newSqlSources newFilters newValueBindings source current toProcess

        and implPatternPredicate sqlSources filters valueBindings source current toProcess =
            let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings source current.PatternMatch.Predicate (current.Restriction.Predicate |> IriObject)
            implPatternObject sqlSources newFilters newValueBindings source current toProcess

        and implPatternObject sqlSources filters valueBindings source current toProcess =
            match current.Restriction.Object with
            | ObjectMatch objMatch ->
                let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings source current.PatternMatch.Object objMatch
                implPatternList sqlSources newFilters newValueBindings toProcess

            | RefObjectMatch refObjMatch ->
                let refSubject = refObjMatch.TargetSubjectMap
                let idColumns = findIds refSubject.Value
                let (refSource, newSqlSources) = findSource sqlSources idColumns current.PatternMatch.Object refSubject.TriplesMap.Source
                let (newFilters, newValueBindings) = applyPatternMatch filters valueBindings refSource current.PatternMatch.Object (refSubject.Value |> IriObject)
                let joinCondition =
                    refObjMatch.JoinConditions
                    |> List.map (
                        fun joinCondition ->
                            let childVariable = source |> SqlSource.getColumn joinCondition.ChildColumn |> Column
                            let targetVariable = refSource |> SqlSource.getColumn joinCondition.TargetColumn |> Column
                            EqualVariables(childVariable, targetVariable)
                    )
                    |> Conjunction
                    // TODO: Add normalization here

                implPatternList newSqlSources (joinCondition :: newFilters) newValueBindings toProcess

        implPatternList Map.empty [] Map.empty patterns

    let processSparqlPattern (sparqlPattern: SparqlPattern) =
        // TODO: Add normalization here
        match sparqlPattern with
        | EmptyPattern -> 
            {
                Model = {
                    AlwaysBoundVariables = List.empty
                    OtherVariables = List.empty
                    Sources = [ SingleEmptyResult ]
                    Assignments = List.empty
                    Filters = List.empty 
                }
                Bindings = Map.empty
            }
        | NotMatchingPattern ->
            {
                Model = {
                    AlwaysBoundVariables = List.empty
                    OtherVariables = List.empty
                    Sources = [ NoResult ]
                    Assignments = List.empty
                    Filters = List.empty 
                }
                Bindings = Map.empty
            }
        | NotProcessedTriplePatterns _ ->
            "Encountered NotProcessedTriplePatterns in RelationalAlgebraBuilder"
            |> invalidArg "sparqlPattern"
        | RestrictedTriplePatterns restrictedPatterns ->
            restrictedPatterns
            |> processRestrictedTriplePattern
        | _ ->
            sprintf "Ended with %A" sparqlPattern
            |> invalidOp

    let applyModifiers (modifiers: Modifier list) (inner: BoundCalculusModel) =
        sprintf "Ended with %A with modifiers to add %A" inner modifiers
        |> invalidOp

    member _.buildRelationalQuery (sparqlAlgebra: SparqlQuery) =
        sparqlAlgebra.Query
        |> processSparqlPattern
        |> applyModifiers sparqlAlgebra.Modifiers





