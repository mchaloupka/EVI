namespace Slp.Evi.R2RML

open System

type TriplesMappingSource =
    | Table of string
    | Statement of string

type TermMapValue =
    | IriColumn of string
    | IriTemplate of string
    | IriConstant of Uri

type ParsedLiteralParts = {
    Value: string
    Type: Uri option
    LanguageTag: string option
}

type LiteralValue =
    | LiteralColumn of string
    | LiteralTemplate of string
    | LiteralConstant of ParsedLiteralParts

type ITriplesMapping =
    abstract member SubjectMap: SubjectMapping
    abstract member PredicateObjectMaps: PredicateObjectMapping list
    abstract member Source: TriplesMappingSource
    abstract member BaseIri: Uri option

and IriMapping = {
    Value: TermMapValue
    BaseIri: Uri option
    TriplesMap: ITriplesMapping
    IsBlankNode: bool
}

and SubjectMapping = {
    Value: IriMapping
    GraphMaps: IriMapping list
    Classes: Uri list
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
    DataTypeIri: Uri option
    Language: string option
}

and ObjectMapping =
    | Iri of IriMapping
    | Literal of LiteralMapping