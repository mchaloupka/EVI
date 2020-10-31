namespace Slp.Evi.Storage.Core.R2RML

open Slp.Evi.Storage.Core.Common
open Slp.Evi.Storage.Core.Common.Types
open Slp.Evi.Storage.Core.Common.Database

type TriplesMappingSource =
    | Table of ISqlTableSchema
    | Statement of string

type TermMapValue =
    | IriColumn of SqlColumnSchema
    | IriTemplate of MappingTemplate.Template<SqlColumnSchema>
    | IriConstant of Iri

type LiteralValue =
    | LiteralColumn of SqlColumnSchema
    | LiteralTemplate of MappingTemplate.Template<SqlColumnSchema>
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