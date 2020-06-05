module Slp.Evi.Sparql.R2RMLMappingProcessor

open System
open Slp.Evi.R2RML
open Slp.Evi.R2RML.MappingTemplate
open Algebra
open SparqlQueryNormalizer
open VDS.RDF
open Slp.Evi.Common.Types
open Slp.Evi.Common.Database
open Slp.Evi.Common.StringRestriction
open Slp.Evi.Common

let private canTemplatesMatch (isIriMatch:bool) (leftTemplate:Template<ISqlColumnSchema>) (rightTemplate:Template<ISqlColumnSchema>) =
    let templateToStringRestriction (input: Template<ISqlColumnSchema>) =
        fun (current: TemplatePart<ISqlColumnSchema>) processed ->
            match current with
            | TextPart t -> RestrictedTemplate.fromText t @ processed
            | ColumnPart c -> (c.SqlType.DefaultRdfType |> LiteralValueType.valueStringRestriction) @ processed
        |> List.foldBack <|| (input, [])
    
    let rec processResult =
        function
        | [] -> true
        | AlwaysMatching :: xs -> processResult xs
        | AlwaysNotMatching :: _ -> false
        | MatchingCondition(leftTemplate, rightTemplate) :: xs ->
            let leftRestriction = leftTemplate |> templateToStringRestriction
            let rightRestriction = rightTemplate |> templateToStringRestriction
            if RestrictedTemplate.canMatch leftRestriction rightRestriction then
                processResult xs
            else
                false
      
    compareTemplates isIriMatch leftTemplate rightTemplate
    |> processResult

let private buildTemplateFromIriMapping = function
    | IriColumn column -> column |> ColumnPart |> List.singleton
    | IriConstant constant -> constant |> Iri.toText |> TextPart |> List.singleton
    | IriTemplate template -> template

let private buildTemplateFromLiteralMapping = function
    | LiteralColumn column -> column |> ColumnPart |> List.singleton
    | LiteralConstant constant -> constant |> TextPart |> List.singleton
    | LiteralTemplate template -> template

let private canIriMatchIriMapping (iri: IUriNode) (mapping: IriMapping) =
    if iri.NodeType = NodeType.Blank && not mapping.IsBlankNode then
        false
    elif iri.NodeType <> NodeType.Blank && mapping.IsBlankNode then
        false
    else
        let nodeUri = iri.Uri.AbsoluteUri |> TextPart |> List.singleton
        let mappingPattern = mapping.Value |> buildTemplateFromIriMapping
        canTemplatesMatch true nodeUri mappingPattern

let private canLiteralMatchLiteralMapping (literal: ILiteralNode) (mapping: LiteralMapping) =
    let literalType = literal |> LiteralValueType.fromLiteralNode

    if mapping.Type <> literalType then
        false
    else
        let literalPattern = literal.Value |> TextPart |> List.singleton
        let mappingPattern = mapping.Value |> buildTemplateFromLiteralMapping
        canTemplatesMatch false literalPattern mappingPattern

let private canMappingMatchPattern (pattern: Pattern) (mapping: ObjectMapping) =
    match pattern, mapping with
    | NodeMatchPattern (IriNode iri), IriObject iriMapping ->
        canIriMatchIriMapping iri iriMapping
    | NodeMatchPattern (LiteralNode valuedNode), LiteralObject literalMapping ->
        canLiteralMatchLiteralMapping valuedNode literalMapping
    | NodeMatchPattern(_), _ -> false
    | _ -> true

let private canMappingMatchTriplePattern (pattern: BasicGraphPatternMatch) (mapping: BasicGraphPatternMapping) =
    if canMappingMatchPattern pattern.Subject (mapping.Subject.Value |> IriObject)
        && canMappingMatchPattern pattern.Predicate (mapping.Predicate |> IriObject) then

        match mapping.Object with
        | ObjectMatch objMapping -> canMappingMatchPattern pattern.Object objMapping
        | RefObjectMatch refObjMapping -> canMappingMatchPattern pattern.Object (refObjMapping.TargetSubjectMap.Value |> IriObject)
    else
        false

let private canMappingsMatch = function
    | IriObject leftMapping, IriObject rightMapping ->
        if leftMapping.IsBlankNode <> rightMapping.IsBlankNode then
            false
        else
            let leftTemplate = leftMapping.Value |> buildTemplateFromIriMapping
            let rightTemplate = rightMapping.Value |> buildTemplateFromIriMapping
            canTemplatesMatch true leftTemplate rightTemplate
    | LiteralObject leftMapping, LiteralObject rightMapping ->
        if leftMapping.Type <> rightMapping.Type then
            false
        else
            let leftTemplate = leftMapping.Value |> buildTemplateFromLiteralMapping
            let rightTemplate = rightMapping.Value |> buildTemplateFromLiteralMapping
            canTemplatesMatch false leftTemplate rightTemplate
    | _, _ ->
        false

let private canRestrictionsMatch (left: Map<SparqlVariable, ObjectMapping>) (right: Map<SparqlVariable, ObjectMapping>) =
    left
    |> Map.forall (
        fun leftKey leftMapping ->
            match right.TryGetValue leftKey with
            | false, _ -> true
            | true, rightMapping ->
                canMappingsMatch (leftMapping, rightMapping)
    )

let private restrictTriplePatterns (restrictBy: RestrictedPatternMatch) (toProcess: (BasicGraphPatternMatch * BasicGraphPatternMapping list) list) =
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

let rec private processTriplePatternsJoin (toProcess: (BasicGraphPatternMatch * BasicGraphPatternMapping list) list): RestrictedPatternMatch list list =
    match toProcess |> List.sortBy (snd >> List.length) with
    | (patternMatch, mappings) :: other ->
        mappings
        |> List.map (
            fun current ->
                let restrictedPatternMatch =
                    { PatternMatch = patternMatch; Restriction = current }

                other
                |> restrictTriplePatterns restrictedPatternMatch
                |> processTriplePatternsJoin
                |> List.map (fun otherResult -> restrictedPatternMatch :: otherResult)
        )
        |> List.concat
    | [] -> [[]]

let private processTriplePatterns (mappings: BasicGraphPatternMapping list) (triplePatterns: BasicGraphPatternMatch list) =
    let addMappingsToMatches (pattern: BasicGraphPatternMatch) =
        pattern, mappings |> List.filter (canMappingMatchTriplePattern pattern)
    
    let patternsWithMappings =
        triplePatterns
        |> List.map addMappingsToMatches
        |> processTriplePatternsJoin

    raise (new System.Exception(sprintf "Ended with %A" patternsWithMappings))

let processSparqlQuery (mapping: BasicGraphPatternMapping list) (input: SparqlQuery) =
    let rec processSparqlPattern = function
        | EmptyPattern -> EmptyPattern
        | ExtendPattern(inner, assignments) -> ExtendPattern(inner |> processSparqlPattern, assignments) |> normalizeSparqlPattern
        | FilterPattern(inner, condition) -> FilterPattern(inner |> processSparqlPattern, condition) |> normalizeSparqlPattern
        | JoinPattern(inners) -> JoinPattern(inners |> List.map processSparqlPattern) |> normalizeSparqlPattern
        | LeftJoinPattern(left, right, condition) -> LeftJoinPattern(left |> processSparqlPattern, right |> processSparqlPattern, condition) |> normalizeSparqlPattern
        | NotMatchingPattern -> NotMatchingPattern
        | UnionPattern(inners) -> UnionPattern(inners |> List.map processSparqlPattern) |> normalizeSparqlPattern
        | RestrictedTriplePatterns(_) -> "Unexpected occurrence of restricted triple pattern" |> invalidOp
        | NotProcessedTriplePatterns(patterns) -> patterns |> processTriplePatterns mapping

    { input with Query = input.Query |> processSparqlPattern }

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