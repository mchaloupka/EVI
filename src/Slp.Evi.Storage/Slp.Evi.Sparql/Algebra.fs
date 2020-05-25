module Slp.Evi.Sparql.Algebra

open Slp.Evi.Common.Algebra
open VDS.RDF
open Slp.Evi.R2RML
open VDS.RDF.Nodes

type SparqlVariable = 
    | SparqlVariable of string
    | BlankNodeVariable of string

type OrderingPart = { Variable: SparqlVariable; Direction: OrderingDirection }

type Node =
    | Literal of IValuedNode
    | Iri of IUriNode

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
