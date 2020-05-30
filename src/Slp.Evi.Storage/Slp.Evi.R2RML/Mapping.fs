namespace Slp.Evi.R2RML

open System
open Slp.Evi.Common.Types

type TriplesMappingSource =
    | Table of string
    | Statement of string

type TermMapValue =
    | IriColumn of string
    | IriTemplate of MappingTemplate.Template
    | IriConstant of Uri

type LiteralValue =
    | LiteralColumn of string
    | LiteralTemplate of MappingTemplate.Template
    | LiteralConstant of string

type ITriplesMapping =
    abstract member SubjectMap: SubjectMapping
    abstract member PredicateObjectMaps: PredicateObjectMapping list
    abstract member Source: TriplesMappingSource
    abstract member BaseIri: Uri option

and IriMapping = {
    Value: TermMapValue
    BaseIri: Uri option
    IsBlankNode: bool
}

and SubjectMapping = {
    Value: IriMapping
    GraphMaps: IriMapping list
    Classes: Uri list
    TriplesMap: ITriplesMapping
}

and PredicateObjectMapping = {
    BaseIri: Uri option
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