namespace Slp.Evi.Sparql

open System
open Slp.Evi.R2RML
open Slp.Evi.R2RML.MappingTemplate
open Algebra
open SparqlQueryNormalizer
open VDS.RDF
open Slp.Evi.Common
open Slp.Evi.Common.Database
open Slp.Evi.Common.Types
open Slp.Evi.Common.ValueRestriction

module R2RMLMappingProcessor =
    let generateBasicGraphPatternMapping (input: ITriplesMapping list): BasicGraphPatternMapping list =
        let classPredicateIri = 
            "http://www.w3.org/1999/02/22-rdf-syntax-ns#type"
            |> Uri
            |> Iri.fromUri
    
        let getGraphMaps graphs =
            if graphs |> List.isEmpty |> not then
                graphs |> List.map Some
            else
                [ None ]

        [ for triplesMapping in input do
            let subjectMap = triplesMapping.SubjectMap
            let baseGraphs = subjectMap.GraphMaps

            for classUri in triplesMapping.SubjectMap.Classes do
                let graphMaps =
                    baseGraphs
                    |> getGraphMaps

                for graphMap in graphMaps do
                    yield {
                        TriplesMap = triplesMapping
                        Subject = subjectMap
                        Predicate = { IsBlankNode = false; BaseIri = triplesMapping.BaseIri; Value = classPredicateIri |> IriConstant }
                        Object = { IsBlankNode = false; BaseIri = triplesMapping.BaseIri; Value = classUri |> IriConstant } |> IriObject |> ObjectMatch
                        Graph = graphMap
                    }

            for predicateObjectMapping in triplesMapping.PredicateObjectMaps do
                let graphMaps =
                    predicateObjectMapping.GraphMaps @ baseGraphs
                    |> getGraphMaps

                for graphMap in graphMaps do
                    for predicateMap in predicateObjectMapping.PredicateMaps do
                        for objectMap in predicateObjectMapping.ObjectMaps do
                            yield { 
                                TriplesMap = triplesMapping
                                Subject = subjectMap
                                Predicate = predicateMap
                                Object = objectMap |> ObjectMatch 
                                Graph = graphMap
                            }

                        for refObjectMap in predicateObjectMapping.RefObjectMaps do
                            yield {
                                TriplesMap = triplesMapping
                                Subject = subjectMap
                                Predicate = predicateMap
                                Object = refObjectMap |> RefObjectMatch
                                Graph = graphMap
                            }
        ]

type R2RMLMappingProcessor(mappings: BasicGraphPatternMapping list) =
    let templateToValueRestriction (isIriMatch:bool) (input: Template<ISqlColumnSchema>) =
        (TemplateFsm.initialMachine (), input)
        ||> List.fold (
            fun fsm part ->
                match part with
                | TextPart t -> TemplateFsm.machineForText t |> TemplateFsm.appendMachine <| fsm
                | ColumnPart c ->
                    c.SqlType.DefaultRdfType 
                    |> LiteralValueType.valueStringRestriction isIriMatch
                    |> TemplateFsm.appendMachine <| fsm
        )
        |> TemplateFsm.finalizeMachine

    let buildTemplateFromIriMapping = function
        | IriColumn column -> None
        | IriConstant constant -> constant |> Iri.toText |> TextPart |> List.singleton |> Some
        | IriTemplate template -> template |> Some
    
    let buildTemplateFromLiteralMapping = function
        | LiteralColumn column -> None
        | LiteralConstant constant -> constant |> TextPart |> List.singleton |> Some
        | LiteralTemplate template -> template |> Some

    let buildCombinations (restrictions: Collections.Generic.IReadOnlyDictionary<'T, TemplateFsm>) =
        let keys = restrictions.Keys |> List.ofSeq

        keys
        |> List.map (
            fun k1 ->
                let m1 = restrictions.[k1]
                let matchingKeys =
                    (List.empty, keys)
                    ||> List.fold (
                        fun cur k2 ->
                            let m2 = restrictions.[k2]
                            if TemplateFsm.canAcceptSameText m1 m2 then
                                k2 :: cur
                            else
                                cur
                    )

                k1, matchingKeys
        )
        |> readOnlyDict

    let iriMappingRestrictions =
        mappings
        |> List.collect (
            fun mapping ->
                let sub = mapping.Subject.Value.Value
                let pred = mapping.Predicate.Value

                match mapping.Object with
                | ObjectMatch(IriObject(obj)) -> [ sub; pred; obj.Value ]
                | RefObjectMatch refObj -> [ sub; pred; refObj.TargetSubjectMap.Value.Value ]
                | _ -> [ sub; pred ]
        )
        |> List.distinct
        |> List.choose (
            fun termMapValue ->
                termMapValue
                |> buildTemplateFromIriMapping
                |> Option.map (
                    fun template ->
                        termMapValue, template |> templateToValueRestriction true
                )
        )
        |> readOnlyDict

    let iriMappingCombinations =
        iriMappingRestrictions |> buildCombinations

    let literalMappingRestrictions =
        mappings
        |> List.collect (fun mapping ->
            match mapping.Object with
            | ObjectMatch(LiteralObject(obj)) -> [ obj.Value ]
            | _ -> List.empty
        )
        |> List.distinct
        |> List.choose (
            fun literalMapValue ->
                literalMapValue
                |> buildTemplateFromLiteralMapping
                |> Option.map (
                    fun template ->
                        literalMapValue, template |> templateToValueRestriction false
                )
        )
        |> readOnlyDict

    let literalMappingCombinations =
        literalMappingRestrictions |> buildCombinations

    let canIriMatchIriMapping (iri: IUriNode) (mapping: IriMapping) =
        if iri.NodeType = NodeType.Blank && not mapping.IsBlankNode then
            false
        elif iri.NodeType <> NodeType.Blank && mapping.IsBlankNode then
            false
        else
            match mapping.Value with
            | IriColumn _ -> true
            | _ ->
                let nodeUri = iri.Uri.AbsoluteUri
                let mappingPattern = iriMappingRestrictions.[mapping.Value]
                mappingPattern |> TemplateFsm.accepts (nodeUri |> List.ofSeq)

    let canLiteralMatchLiteralMapping (literal: ILiteralNode) (mapping: LiteralMapping) =
        let literalType = literal |> LiteralValueType.fromLiteralNode

        if mapping.Type <> literalType then
            false
        else
            match mapping.Value with
            | LiteralColumn _ -> true
            | _ ->
                let literalPattern = literal.Value
                let mappingPattern = literalMappingRestrictions.[mapping.Value]
                mappingPattern |> TemplateFsm.accepts (literalPattern |> List.ofSeq)

    let canMappingMatchPattern (pattern: Pattern) (mapping: ObjectMapping) =
        match pattern, mapping with
        | NodeMatchPattern (IriNode iri), IriObject iriMapping ->
            canIriMatchIriMapping iri iriMapping
        | NodeMatchPattern (LiteralNode valuedNode), LiteralObject literalMapping ->
            canLiteralMatchLiteralMapping valuedNode literalMapping
        | NodeMatchPattern(_), _ -> false
        | _ -> true

    let canMappingMatchTriplePattern (pattern: BasicGraphPatternMatch) (mapping: BasicGraphPatternMapping) =
        if canMappingMatchPattern pattern.Subject (mapping.Subject.Value |> IriObject)
            && canMappingMatchPattern pattern.Predicate (mapping.Predicate |> IriObject) then

            match mapping.Object with
            | ObjectMatch objMapping -> canMappingMatchPattern pattern.Object objMapping
            | RefObjectMatch refObjMapping -> canMappingMatchPattern pattern.Object (refObjMapping.TargetSubjectMap.Value |> IriObject)
        else
            false

    let canMappingsMatch = function
        | IriObject leftMapping, IriObject rightMapping ->
            if leftMapping.IsBlankNode <> rightMapping.IsBlankNode then
                false
            else
                match leftMapping.Value, rightMapping.Value with
                | IriColumn _, _
                | _, IriColumn _ ->
                    true
                | _, _ ->
                    iriMappingCombinations.[leftMapping.Value]
                    |> List.contains (rightMapping.Value)

        | LiteralObject leftMapping, LiteralObject rightMapping ->
            if leftMapping.Type <> rightMapping.Type then
                false
            else
                match leftMapping.Value, rightMapping.Value with
                | LiteralColumn _, _
                | _, LiteralColumn _ ->
                    true
                | _, _ ->
                    literalMappingCombinations.[leftMapping.Value]
                    |> List.contains (rightMapping.Value)

        | _, _ ->
            false

    let canRestrictionsMatch (left: Map<SparqlVariable, ObjectMapping>) (right: Map<SparqlVariable, ObjectMapping>) =
        left
        |> Map.forall (
            fun leftKey leftMapping ->
                match right.TryGetValue leftKey with
                | false, _ -> true
                | true, rightMapping ->
                    canMappingsMatch (leftMapping, rightMapping)
        )

    let restrictTriplePatterns (restrictBy: RestrictedPatternMatch) (toProcess: (BasicGraphPatternMatch * BasicGraphPatternMapping list) list) =
        let extractVariableConstraints (patternMatch: BasicGraphPatternMatch) (mapping: BasicGraphPatternMapping) =
            [
                patternMatch.Subject, mapping.Subject.Value |> IriObject
                patternMatch.Predicate, mapping.Predicate |> IriObject
                patternMatch.Object, mapping.Object |> (function | ObjectMatch x -> x | RefObjectMatch x -> x.TargetSubjectMap.Value |> IriObject)
            ]
            |> List.choose (
                function
                | VariablePattern v, mapping -> Some(v, mapping)
                | _ -> None
            )
            |> Map.ofList

        let restrictions = extractVariableConstraints restrictBy.PatternMatch restrictBy.Restriction
    
        toProcess
        |> List.map (
            fun (patternMatch, mappings) ->
                let filteredMappings =
                    mappings
                    |> List.filter ((extractVariableConstraints patternMatch) >> (canRestrictionsMatch restrictions))
            
                patternMatch, filteredMappings
        )

    let rec processTriplePatternsJoin (toProcess: (BasicGraphPatternMatch * BasicGraphPatternMapping list) list): RestrictedPatternMatch list list =
        match toProcess |> List.sortBy (snd >> List.length) with
        | (patternMatch, mappings) :: other ->
            mappings
            |> List.collect (
                fun current ->
                    let restrictedPatternMatch =
                        { PatternMatch = patternMatch; Restriction = current }

                    other
                    |> restrictTriplePatterns restrictedPatternMatch
                    |> processTriplePatternsJoin
                    |> List.map (fun otherResult -> restrictedPatternMatch :: otherResult)
            )
        | [] -> [[]]

    let processTriplePatterns (triplePatterns: BasicGraphPatternMatch list) =
        let addMappingsToMatches (pattern: BasicGraphPatternMatch) =
            pattern, mappings |> List.filter (canMappingMatchTriplePattern pattern)
    
        triplePatterns
        |> List.map addMappingsToMatches
        |> processTriplePatternsJoin
        |> List.map RestrictedTriplePatterns
        |> List.map normalizeSparqlPattern
        |> UnionPattern
        |> normalizeSparqlPattern

    let rec processSparqlPattern = function
        | EmptyPattern -> EmptyPattern
        | ExtendPattern(inner, assignments) -> ExtendPattern(inner |> processSparqlPattern, assignments) |> normalizeSparqlPattern
        | FilterPattern(inner, condition) -> FilterPattern(inner |> processSparqlPattern, condition) |> normalizeSparqlPattern
        | JoinPattern(inners) -> JoinPattern(inners |> List.map processSparqlPattern) |> normalizeSparqlPattern
        | LeftJoinPattern(left, right, condition) -> LeftJoinPattern(left |> processSparqlPattern, right |> processSparqlPattern, condition) |> normalizeSparqlPattern
        | NotMatchingPattern -> NotMatchingPattern
        | UnionPattern(inners) -> UnionPattern(inners |> List.map processSparqlPattern) |> normalizeSparqlPattern
        | RestrictedTriplePatterns(_) -> "Unexpected occurrence of restricted triple pattern" |> invalidOp
        | NotProcessedTriplePatterns(patterns) -> patterns |> processTriplePatterns

    member _.processSparqlQuery input = { input with Query = input.Query |> processSparqlPattern }