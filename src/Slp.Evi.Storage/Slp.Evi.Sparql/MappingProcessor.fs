module Slp.Evi.Sparql.R2RMLMappingProcessor

open Slp.Evi.R2RML
open Slp.Evi.R2RML.MappingTemplate
open Algebra
open SparqlQueryNormalizer
open VDS.RDF
open Slp.Evi.Common.Types

let canTemplatesMatch (isIriMatch:bool) (leftTemplate:Template<_>) (rightTemplate:Template<_>) =
    compareTemplates isIriMatch leftTemplate rightTemplate
    |> TemplateCompareResult.isNeverMatching
    |> not

let canIriMatchIriMapping (iri: IUriNode) (mapping: IriMapping) =
    if iri.NodeType = NodeType.Blank && not mapping.IsBlankNode then
        false
    elif iri.NodeType <> NodeType.Blank && mapping.IsBlankNode then
        false
    else
        let nodeUri = iri.Uri.AbsoluteUri |> TextPart |> List.singleton
        let mappingPattern =
            match mapping.Value with
            | IriColumn column -> column |> ColumnPart |> List.singleton
            | IriConstant constant -> constant.AbsoluteUri |> TextPart |> List.singleton
            | IriTemplate template -> template

        canTemplatesMatch true nodeUri mappingPattern

let canLiteralMatchLiteralMapping (literal: ILiteralNode) (mapping: LiteralMapping) =
    let literalType = literal |> LiteralValueType.fromLiteralNode

    if mapping.Type <> literalType then
        false
    else
        let literalPattern = literal.Value |> TextPart |> List.singleton
        let mappingPattern =
            match mapping.Value with
            | LiteralColumn column -> column |> ColumnPart |> List.singleton
            | LiteralConstant constant -> constant |> TextPart |> List.singleton
            | LiteralTemplate template -> template

        canTemplatesMatch false literalPattern mappingPattern

let canMappingMatchPattern (pattern: Pattern) (mapping: ObjectMapping) =
    match pattern, mapping with
    | NodeMatchPattern (Iri iri), IriObject iriMapping ->
        canIriMatchIriMapping iri iriMapping
    | NodeMatchPattern (Literal valuedNode), LiteralObject literalMapping ->
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

let private processTriplePatterns (mappings: BasicGraphPatternMapping list) (triplePatterns: BasicGraphPatternMatch list) =
    let addMappingsToMatches (pattern: BasicGraphPatternMatch) =
        pattern, mappings |> List.filter (canMappingMatchTriplePattern pattern)
    
    let patternsWithMappings =
        triplePatterns
        |> List.map addMappingsToMatches

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
        |> VDS.RDF.UriFactory.Create
    
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