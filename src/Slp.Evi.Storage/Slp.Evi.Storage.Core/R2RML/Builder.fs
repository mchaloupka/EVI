module Slp.Evi.Storage.Core.R2RML.Builder

open System
open TCode.r2rml4net
open TCode.r2rml4net.Extensions
open TCode.r2rml4net.Mapping
open VDS.RDF
open Slp.Evi.Storage.Core.Common.Database
open Slp.Evi.Storage.Core.Common.Types
open Slp.Evi.Storage.Core.Common.Utils
open Slp.Evi.Storage.Core.Common

let private parseTemplate (tableSchema: ISqlTableSchema) = MappingTemplate.parseTemplate tableSchema.GetColumn

let private parseLiteralParts (literalTermMap: ILiteralTermMap): string * LiteralValueType =
    let node = 
        literalTermMap.Node.GetObjects("rr:constant")
        |> Seq.tryHead 
        |> Option.map (function
            | :? ILiteralNode as literalNode -> literalNode
            | _ -> "The literal value has incorrect type" |> invalidOp
        )

    match node with
    | Some(x) ->
        let dataType = createOptionFromNullable x.DataType
        let language = createOptionFromNullable x.Language

        let literalType = 
            match dataType, language with
            | Some(_), Some(_) -> "Literal has both data type and language set" |> invalidOp
            | Some(iri), _ -> iri |> Iri.fromUri |> WithType
            | _, Some(lang) -> lang |> WithLanguage
            | _, _ -> DefaultType
        

        x.Value, literalType
    | None -> "The literal value is missing" |> invalidOp

let private createIriMapping (tableSchema: ISqlTableSchema) (triplesBaseIri: Iri option) (termMap: IUriValuedTermMap): IriMapping =
    let isBlankNode = termMap.TermType.IsBlankNode
    let baseIri = createOptionFromNullable termMap.BaseUri |> Option.map Iri.fromUri |> Option.orElse triplesBaseIri

    let value =
        if termMap.IsConstantValued then termMap.URI |> IriReference.fromUri |> IriReference.tryResolve baseIri |> IriConstant
        else if termMap.IsTemplateValued then termMap.Template |> parseTemplate tableSchema |> IriTemplate
        else if termMap.IsColumnValued then termMap.ColumnName |> tableSchema.GetColumn |> IriColumn 
        else "Unsupported term type" |> invalidOp

    { Value = value; BaseIri = baseIri; IsBlankNode = isBlankNode }

let private createObjectMapping (tableSchema: ISqlTableSchema) (triplesBaseIri: Iri option) (objectMap: IObjectMap): ObjectMapping =
    if objectMap.TermType.IsLiteral then
        let (detectedType, value) =
            if objectMap.IsConstantValued then
                let parsedValue, parsedType = parseLiteralParts objectMap
                parsedType, LiteralConstant parsedValue
            else if objectMap.IsTemplateValued then
                DefaultType, objectMap.Template |> parseTemplate tableSchema |> LiteralTemplate
            else if objectMap.IsColumnValued then
                let columnName = objectMap.ColumnName
                tableSchema.GetColumn(columnName).SqlType.DefaultRdfType, columnName |> tableSchema.GetColumn |> LiteralColumn
            else raise (new NotSupportedException("Unsupported term type"))

        let dataTypeIri = createOptionFromNullable objectMap.DataTypeURI
        let language = createOptionFromNullable objectMap.Language

        let literalType = 
            match (dataTypeIri, language, detectedType) with
            | Some(_), Some(_), _ -> "Cannot have both language and type on a literal set" |> invalidOp
            | Some(iri), _, _ -> iri |> Iri.fromUri |> WithType
            | _, Some(language), _-> language |> WithLanguage
            | _, _, detectedType -> detectedType

        LiteralObject { Value = value; Type = literalType }
    else
        let isBlankNode = objectMap.TermType.IsBlankNode
        let baseIri = createOptionFromNullable objectMap.BaseUri |> Option.map Iri.fromUri |> Option.orElse triplesBaseIri

        let value =
            if objectMap.IsConstantValued then objectMap.URI |> IriReference.fromUri |> IriReference.tryResolve baseIri |> IriConstant
            else if objectMap.IsTemplateValued then objectMap.Template |> parseTemplate tableSchema |> IriTemplate
            else if objectMap.IsColumnValued then objectMap.ColumnName |> tableSchema.GetColumn |> IriColumn
            else raise (new NotSupportedException("Unsupported term type"))

        IriObject { Value = value; BaseIri = baseIri; IsBlankNode = isBlankNode }

let private createSubjectMap (tableSchema: ISqlTableSchema) (subjectMap: ISubjectMap) (triplesMapping: ITriplesMapping): SubjectMapping =
    let value = createIriMapping tableSchema triplesMapping.BaseIri subjectMap
    let graphMaps = subjectMap.GraphMaps |> Seq.map (createIriMapping tableSchema triplesMapping.BaseIri) |> Seq.toList
    let classes =
        subjectMap.Classes 
        |> Array.toList
        |> List.map (IriReference.fromUri >> IriReference.tryResolve triplesMapping.BaseIri)

    { Value = value; GraphMaps = graphMaps; Classes = classes; TriplesMap = triplesMapping }

type private TriplesMapping private(source: TriplesMappingSource, baseIri: Iri option) =
    let mutable predicateObjectMaps: List<PredicateObjectMapping> = []
    let mutable subjectMap: SubjectMapping option = None

    new (databaseSchema: ISqlDatabaseSchema, triplesMap: ITriplesMap) =
        let source =
            if String.IsNullOrEmpty(triplesMap.TableName) then Statement triplesMap.SqlQuery
            else triplesMap.TableName |> databaseSchema.GetTable |> Table

        let baseIri = createOptionFromNullable triplesMap.BaseUri |> Option.map Iri.fromUri

        TriplesMapping(source, baseIri)

    member _.BaseIri = baseIri

    member _.Source = source

    member _.SubjectMap
        with get() = subjectMap.Value
        and set(value) = subjectMap <- value |> Some

    member _.PredicateObjectMaps
        with get() = predicateObjectMaps
        and set(value) = predicateObjectMaps <- value

    interface ITriplesMapping with
        member this.BaseIri = this.BaseIri
        member this.PredicateObjectMaps = this.PredicateObjectMaps
        member this.Source = this.Source
        member this.SubjectMap = this.SubjectMap

let createMappingRepresentation (databaseSchema: ISqlDatabaseSchema) (mappingInput: IR2RML): ITriplesMapping list =
    let getTableSchema (input: TriplesMapping) =
        match input.Source with
        | Table tableSchema -> tableSchema
        | _ -> sprintf "The source %A is not yet supported for schema inferring" input.Source |> NotImplementedException |> raise
 
    let tripleMapsToMapping =
        seq { for tripleMap in mappingInput.TriplesMaps -> tripleMap, new TriplesMapping(databaseSchema, tripleMap) }
        |> Seq.map (fun (tripleMap, triplesMapping) ->
            triplesMapping.SubjectMap <- createSubjectMap (getTableSchema triplesMapping) (tripleMap.SubjectMap) triplesMapping
            tripleMap, triplesMapping
        )
        |> readOnlyDict

    let fillTriplesMapping (tripleMap: ITriplesMap, triplesMapping: TriplesMapping) =
        let tableSchema = getTableSchema triplesMapping

        let createRefObjectMap (refObjectMap: IRefObjectMap) =
            let targetTriplesMap = refObjectMap.SubjectMap.TriplesMap
            let targetSubject = tripleMapsToMapping.[targetTriplesMap].SubjectMap
            let joinConditions =
                refObjectMap.JoinConditions
                |> Seq.map (fun condition -> { ChildColumn = condition.ChildColumn; TargetColumn = condition.ParentColumn })
                |> Seq.toList

            { TargetSubjectMap = targetSubject; JoinConditions = joinConditions }

        let createPredicateObjectMap (predicateObjectMap: IPredicateObjectMap) =
            let baseIri = createOptionFromNullable predicateObjectMap.BaseUri |> Option.map Iri.fromUri |> Option.orElse triplesMapping.BaseIri
            let graphMaps = predicateObjectMap.GraphMaps |> Seq.map (createIriMapping tableSchema baseIri) |> Seq.toList
            let predicateMaps = predicateObjectMap.PredicateMaps |> Seq.map (createIriMapping tableSchema baseIri) |> Seq.toList
            let refObjectMaps = predicateObjectMap.RefObjectMaps |> Seq.map createRefObjectMap |> Seq.toList
            let objectMaps = predicateObjectMap.ObjectMaps |> Seq.map (createObjectMapping tableSchema baseIri) |> Seq.toList

            { BaseIri = baseIri; GraphMaps = graphMaps; PredicateMaps = predicateMaps; ObjectMaps = objectMaps; RefObjectMaps = refObjectMaps }

        let predicateObjectMaps =
            tripleMap.PredicateObjectMaps
            |> Seq.map createPredicateObjectMap
            |> Seq.toList

        triplesMapping.PredicateObjectMaps <- predicateObjectMaps

    tripleMapsToMapping |> Seq.map (fun x -> x.Key, x.Value) |> Seq.iter fillTriplesMapping
    tripleMapsToMapping.Values |> Seq.cast |> Seq.toList
