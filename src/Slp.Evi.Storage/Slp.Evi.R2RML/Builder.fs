module Slp.Evi.R2RML.Builder

open System
open TCode.r2rml4net
open TCode.r2rml4net.Extensions
open TCode.r2rml4net.Mapping
open VDS.RDF

let private createOptionFromNullable x = if x = null then None else Some x

let private parseLiteralParts (literalTermMap: ILiteralTermMap): ParsedLiteralParts =
    let node = 
        literalTermMap.Node.GetObjects("rr:constant")
        |> Seq.tryHead 
        |> Option.map (function
            | :? ILiteralNode as literalNode -> literalNode
            | _ -> raise (new InvalidOperationException("The literal value has incorrect type"))
        )

    match node with
    | Some(x) ->
        let dataType = createOptionFromNullable x.DataType
        let language = createOptionFromNullable x.Language
        { Value = x.Value; Type = dataType; LanguageTag = language }
    | None -> raise (new InvalidOperationException("The literal value is missing"))

let private createIriMapping (termMap: IUriValuedTermMap): IriMapping =
    let isBlankNode = termMap.TermType.IsBlankNode
    let baseIri = createOptionFromNullable termMap.BaseUri

    let value =
        if termMap.IsConstantValued then IriConstant termMap.URI
        else if termMap.IsTemplateValued then termMap.Template |> MappingTemplate.parseTemplate |> IriTemplate
        else if termMap.IsColumnValued then IriColumn termMap.ColumnName
        else raise (new NotSupportedException("Unsupported term type"))

    { Value = value; BaseIri = baseIri; IsBlankNode = isBlankNode }

let private createObjectMapping (objectMap: IObjectMap): ObjectMapping =
    if objectMap.TermType.IsLiteral then
        let value =
            if objectMap.IsConstantValued then LiteralConstant (parseLiteralParts objectMap)
            else if objectMap.IsTemplateValued then objectMap.Template |> MappingTemplate.parseTemplate |> LiteralTemplate
            else if objectMap.IsColumnValued then LiteralColumn objectMap.ColumnName
            else raise (new NotSupportedException("Unsupported term type"))

        let dataTypeIri = createOptionFromNullable objectMap.DataTypeURI
        let language = createOptionFromNullable objectMap.Language

        LiteralObject { Value = value; DataTypeIri = dataTypeIri; Language = language }
    else
        let isBlankNode = objectMap.TermType.IsBlankNode
        let baseIri = createOptionFromNullable objectMap.BaseUri

        let value =
            if objectMap.IsConstantValued then IriConstant objectMap.URI
            else if objectMap.IsTemplateValued then objectMap.Template |> MappingTemplate.parseTemplate |> IriTemplate
            else if objectMap.IsColumnValued then IriColumn objectMap.ColumnName
            else raise (new NotSupportedException("Unsupported term type"))

        IriObject { Value = value; BaseIri = baseIri; IsBlankNode = isBlankNode }

let private createSubjectMap (subjectMap: ISubjectMap) (triplesMapping: ITriplesMapping): SubjectMapping =
    let value = createIriMapping subjectMap
    let graphMaps = subjectMap.GraphMaps |> Seq.map createIriMapping |> Seq.toList
    let classes = subjectMap.Classes |> Array.toList

    { Value = value; GraphMaps = graphMaps; Classes = classes; TriplesMap = triplesMapping }

type private TriplesMapping private(source: TriplesMappingSource, baseIri: Option<Uri>) =
    let mutable predicateObjectMaps: List<PredicateObjectMapping> = []
    let mutable subjectMap: SubjectMapping option = None

    new (triplesMap: ITriplesMap) =
        let source =
            if String.IsNullOrEmpty(triplesMap.TableName) then Statement triplesMap.SqlQuery
            else Table triplesMap.TableName

        let baseIri = createOptionFromNullable triplesMap.BaseUri

        TriplesMapping(source, baseIri)

    member this.BaseIri = baseIri

    member this.Source = source

    member this.SubjectMap
        with get() = subjectMap.Value
        and set(value) = subjectMap <- value |> Some

    member this.PredicateObjectMaps
        with get() = predicateObjectMaps
        and set(value) = predicateObjectMaps <- value

    interface ITriplesMapping with
        member this.BaseIri = this.BaseIri
        member this.PredicateObjectMaps = this.PredicateObjectMaps
        member this.Source = this.Source
        member this.SubjectMap = this.SubjectMap

let createMappingRepresentation (mappingInput: IR2RML): ITriplesMapping list =
    let tripleMapsToMapping =
        seq { for tripleMap in mappingInput.TriplesMaps -> tripleMap, new TriplesMapping(tripleMap) }
        |> Seq.map (fun (tripleMap, triplesMapping) ->
            triplesMapping.SubjectMap <- createSubjectMap (tripleMap.SubjectMap) triplesMapping
            tripleMap, triplesMapping
        )
        |> readOnlyDict

    let fillTriplesMapping (tripleMap: ITriplesMap, triplesMapping: TriplesMapping) =
        let createRefObjectMap (refObjectMap: IRefObjectMap) =
            let targetTriplesMap = refObjectMap.SubjectMap.TriplesMap
            let targetSubject = tripleMapsToMapping.[targetTriplesMap].SubjectMap
            let joinConditions =
                refObjectMap.JoinConditions
                |> Seq.map (fun condition -> { ChildColumn = condition.ChildColumn; TargetColumn = condition.ParentColumn })
                |> Seq.toList

            { TargetSubjectMap = targetSubject; JoinConditions = joinConditions }

        let createPredicateObjectMap (predicateObjectMap: IPredicateObjectMap) =
            let baseIri = createOptionFromNullable predicateObjectMap.BaseUri
            let graphMaps = predicateObjectMap.GraphMaps |> Seq.map createIriMapping |> Seq.toList
            let predicateMaps = predicateObjectMap.PredicateMaps |> Seq.map createIriMapping |> Seq.toList
            let refObjectMaps = predicateObjectMap.RefObjectMaps |> Seq.map createRefObjectMap |> Seq.toList
            let objectMaps = predicateObjectMap.ObjectMaps |> Seq.map createObjectMapping |> Seq.toList

            { BaseIri = baseIri; GraphMaps = graphMaps; PredicateMaps = predicateMaps; ObjectMaps = objectMaps; RefObjectMaps = refObjectMaps }

        let predicateObjectMaps =
            tripleMap.PredicateObjectMaps
            |> Seq.map createPredicateObjectMap
            |> Seq.toList

        triplesMapping.PredicateObjectMaps <- predicateObjectMaps

    tripleMapsToMapping |> Seq.map (fun x -> x.Key, x.Value) |> Seq.iter fillTriplesMapping
    tripleMapsToMapping.Values |> Seq.cast |> Seq.toList
