namespace Slp.Evi.R2RML

open System
open Slp.Evi.Common
open Slp.Evi.Common.Types
open Slp.Evi.Common.Database

type TriplesMappingSource =
    | Table of ISqlTableSchema
    | Statement of string

type TermMapValue =
    | IriColumn of ISqlColumnSchema
    | IriTemplate of MappingTemplate.Template<ISqlColumnSchema>
    | IriConstant of Iri

type LiteralValue =
    | LiteralColumn of ISqlColumnSchema
    | LiteralTemplate of MappingTemplate.Template<ISqlColumnSchema>
    | LiteralConstant of string

type ITriplesMapping =
    abstract member SubjectMap: SubjectMapping
    abstract member PredicateObjectMaps: PredicateObjectMapping list
    abstract member Source: TriplesMappingSource
    abstract member BaseIri: Iri option

and IriMapping = {
    Value: TermMapValue
    BaseIri: Iri option
    IsBlankNode: bool
}

and SubjectMapping = {
    Value: IriMapping
    GraphMaps: IriMapping list
    Classes: Iri list
    TriplesMap: ITriplesMapping
}

and PredicateObjectMapping = {
    BaseIri: Iri option
    PredicateMaps: IriMapping list
    ObjectMaps: ObjectMapping list
    RefObjectMaps: RefObjectMapping list
    GraphMaps: IriMapping list
}

and RefObjectJoinCondition = {
    ChildColumn: string
    TargetColumn: string
}

and RefObjectMapping = {
    TargetSubjectMap: SubjectMapping
    JoinConditions: RefObjectJoinCondition list
}

and LiteralMapping = {
    Value: LiteralValue
    Type: LiteralValueType
}

and ObjectMapping =
    | IriObject of IriMapping
    | LiteralObject of LiteralMapping