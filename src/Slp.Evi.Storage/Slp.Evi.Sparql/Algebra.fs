module Slp.Evi.Sparql.Algebra

open Slp.Evi.Common
open Slp.Evi.Common.Algebra
open Slp.Evi.Common.Types
open Slp.Evi.R2RML

type SparqlVariable = 
    | SparqlVariable of string
    | BlankNodeVariable of string

type OrderingPart = { Variable: SparqlVariable; Direction: OrderingDirection }

type IriNode = { IsBlankNode: bool; Iri: Iri }

type LiteralNode = { Value: string; ValueType: LiteralValueType }

type Node =
    | LiteralNode of LiteralNode
    | IriNode of IriNode

type Pattern =
    | VariablePattern of SparqlVariable
    | NodeMatchPattern of Node

type BasicGraphPatternMatch = { Subject: Pattern; Predicate: Pattern; Object: Pattern }

type BasicGraphObjectMapping = 
    | ObjectMatch of ObjectMapping
    | RefObjectMatch of RefObjectMapping

type BasicGraphPatternMapping = { 
    TriplesMap: ITriplesMapping
    Subject: SubjectMapping
    Predicate: IriMapping
    Object: BasicGraphObjectMapping
    Graph: IriMapping option
}

type RestrictedPatternMatch = { PatternMatch: BasicGraphPatternMatch; Restriction: BasicGraphPatternMapping }

type SparqlCondition =
    | AlwaysFalseCondition
    | AlwaysTrueCondition
    | ComparisonCondition of Comparisons * SparqlExpression * SparqlExpression
    | ConjunctionCondition of SparqlCondition list
    | DisjunctionCondition of SparqlCondition list
    | IsBoundCondition of SparqlVariable
    | LanguageMatchesCondition of {| Language: SparqlExpression; LanguageRange: SparqlExpression |}
    | NegationCondition of SparqlCondition
    | RegexCondition of {| Expression: SparqlExpression; Pattern: SparqlExpression; Flags: SparqlExpression option |}
    // ...

and SparqlExpression =
    | BinaryArithmeticExpression of ArithmeticOperator * SparqlExpression * SparqlExpression
    | BooleanExpression of SparqlCondition
    | LangExpression of SparqlExpression
    | NodeExpression of Node
    | VariableExpression of SparqlVariable
    // ...

and Modifier =
    | Distinct
    | OrderBy of OrderingPart list
    | Select of SparqlVariable list
    | Slice of {| Limit: int option; Offset: int option |}
    // | Reduced
    // | ... Aggregation

and SparqlPattern =
    | EmptyPattern
    | ExtendPattern of SparqlPattern * (SparqlVariable * SparqlExpression) list
    | FilterPattern of SparqlPattern * SparqlCondition
    | JoinPattern of SparqlPattern list
    | LeftJoinPattern of SparqlPattern * SparqlPattern * SparqlCondition
    | NotMatchingPattern
    | UnionPattern of SparqlPattern list
    | NotProcessedTriplePatterns of BasicGraphPatternMatch list
    | RestrictedTriplePatterns of RestrictedPatternMatch list
    // | GraphPattern
    // | MinusPattern

and SparqlQuery = { Query: SparqlPattern; Modifiers: Modifier list }
