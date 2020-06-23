module Slp.Evi.Sparql.SparqlQueryBuilder

open Algebra
open SparqlQueryNormalizer

open System
open VDS.RDF
open Slp.Evi.Common.Algebra
open System.Reflection
open Slp.Evi.Common.Types
open Slp.Evi.Common

let private getNodeFromConstantTerm: Query.Expressions.Primary.ConstantTerm -> INode =
    let property =
        typeof<Query.Expressions.Primary.ConstantTerm>.GetProperty("Node", BindingFlags.NonPublic ||| BindingFlags.GetProperty ||| BindingFlags.Default ||| BindingFlags.Instance)
    fun (x: Query.Expressions.Primary.ConstantTerm) ->
        property.GetValue(x) :?> INode

let private processNode (orNode: INode) =
    match orNode with
    | :? IUriNode as orUriNode ->
        IriNode { IsBlankNode = orUriNode.NodeType = NodeType.Blank; Iri = Iri.fromUri orUriNode.Uri }
    | :? ILiteralNode as orLiteralNode ->
        Algebra.LiteralNode { Value = orLiteralNode.Value; ValueType = LiteralValueType.fromLiteralNode orLiteralNode }
    | _ ->
        sprintf "Node is expected to be either IUriNode or ILiteralNode but is %A" orNode
        |> NotSupportedException
        |> raise

let rec private processSparqlCondition (vdsExpression: Query.Expressions.ISparqlExpression): SparqlCondition =
    match processSparqlExpression vdsExpression with
    | BooleanExpression condition -> condition
    | NodeExpression(LiteralNode node) ->
        if (node.ValueType = KnownTypes.xsdBoolean && node.Value = "true")
            || (node.ValueType = KnownTypes.xsdInteger && node.Value <> "0") 
            || (node.ValueType = DefaultType && node.Value |> String.length > 0) then
            AlwaysTrueCondition
        else
            AlwaysFalseCondition
    | other ->
        raise (new NotSupportedException(sprintf "Expected a SPARQL condition, found %A instead" other))

and private processSparqlExpression (vdsExpression: Query.Expressions.ISparqlExpression): SparqlExpression =
    normalizeSparqlExpression <|
    match vdsExpression with
    | :? Query.Expressions.Functions.Sparql.Boolean.BoundFunction as orBound ->
        orBound.Variables |> Seq.map SparqlVariable |> Seq.exactlyOne |> IsBoundCondition |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Conditional.AndExpression as orAnd ->
        orAnd.Arguments |> Seq.map processSparqlCondition |> List.ofSeq |> ConjunctionCondition |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Conditional.OrExpression as orOr ->
        orOr.Arguments |> Seq.map processSparqlCondition |> List.ofSeq |> DisjunctionCondition |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Conditional.NotExpression as orNot ->
        orNot.Arguments |> Seq.map processSparqlCondition |> Seq.exactlyOne |> NegationCondition |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Comparison.GreaterThanExpression as orComparison ->
        processComparison GreaterThan orComparison.Arguments |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Comparison.GreaterThanOrEqualToExpression as orComparison ->
        processComparison GreaterOrEqualThan orComparison.Arguments |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Comparison.LessThanExpression as orComparison ->
        processComparison LessThan orComparison.Arguments |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Comparison.LessThanOrEqualToExpression as orComparison ->
        processComparison LessOrEqualThan orComparison.Arguments |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Comparison.EqualsExpression as orComparison ->
        processComparison EqualTo orComparison.Arguments |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Comparison.NotEqualsExpression as orComparison ->
        processComparison NotEqualTo orComparison.Arguments |> normalizeSparqlCondition |> BooleanExpression
    | :? Query.Expressions.Primary.VariableTerm as orVariable ->
        orVariable.Variables |> Seq.exactlyOne |> SparqlVariable |> VariableExpression
    | :? Query.Expressions.Primary.ConstantTerm as orConstant ->
        orConstant |> getNodeFromConstantTerm |> processNode |> NodeExpression
    | :? Query.Expressions.Arithmetic.AdditionExpression as orArithmetic ->
        processArithmetic Add orArithmetic.Arguments
    | :? Query.Expressions.Arithmetic.SubtractionExpression as orArithmetic ->
        processArithmetic Subtract orArithmetic.Arguments
    | :? Query.Expressions.Arithmetic.MultiplicationExpression as orArithmetic ->
        processArithmetic Multiply orArithmetic.Arguments
    | :? Query.Expressions.Arithmetic.DivisionExpression as orArithmetic ->
        processArithmetic Divide orArithmetic.Arguments
    | :? Query.Expressions.Functions.Sparql.Boolean.RegexFunction as orRegex ->
        let arguments = orRegex.Arguments |> Seq.map processSparqlExpression |> List.ofSeq

        if arguments.Length = 2 then
            RegexCondition {| Expression=arguments.[0]; Pattern=arguments.[1]; Flags=None |} |> BooleanExpression
        elif arguments.Length = 3 then
            RegexCondition {| Expression=arguments.[0]; Pattern=arguments.[1]; Flags=Some(arguments.[2]) |} |> BooleanExpression
        else
            raise (new NotSupportedException("Unsupported count of parameters in Regex"))
    | :? Query.Expressions.Functions.Sparql.Boolean.LangMatchesFunction as orLangMatches ->
        let arguments = orLangMatches.Arguments |> Seq.map processSparqlExpression |> List.ofSeq
        
        if arguments.Length = 2 then
            LanguageMatchesCondition({| Language=arguments.[0]; LanguageRange=arguments.[1] |}) |> BooleanExpression
        else
            raise (new NotSupportedException("Unsupported count of parameters in language match condition"))
    | :? Query.Expressions.Functions.Sparql.String.LangFunction as orLang ->
        orLang.Arguments |> Seq.exactlyOne |> processSparqlExpression |> LangExpression
    | _ ->
        raise (new NotImplementedException(sprintf "Expression %A is not supported" vdsExpression))

and private processArithmetic (operator: ArithmeticOperator) (arguments: Query.Expressions.ISparqlExpression seq): SparqlExpression =
    let arguments = arguments |> Seq.map processSparqlExpression |> List.ofSeq
    if arguments.Length = 2 then
        let left = arguments.[0]
        let right = arguments.[1]
        BinaryArithmeticExpression(operator, left, right)
    else
        raise (new NotSupportedException("Unsupported count of parameters in arithmetic expression"))

and private processComparison (comparison: Comparisons) (arguments: Query.Expressions.ISparqlExpression seq): SparqlCondition =
    let arguments = arguments |> Seq.map processSparqlExpression |> List.ofSeq
    if arguments.Length = 2 then
        let left = arguments.[0]
        let right = arguments.[1]
        ComparisonCondition(comparison, left, right)
    else
        raise (new NotSupportedException("Unsupported count of parameters in comparison"))

let rec private processOrderBy (vdsOrderBy: Query.Ordering.ISparqlOrderBy): OrderingPart list =
    let next =
        if (vdsOrderBy.Child <> null) then
            processOrderBy (vdsOrderBy.Child)
        else List.empty

    let current =
        if (vdsOrderBy.IsSimple) then
            match (vdsOrderBy.Expression |> processSparqlExpression) with
            | VariableExpression(variable) ->
                { 
                    Variable=variable
                    Direction=
                        if vdsOrderBy.Descending then
                            Descending
                        else
                            Ascending
                }
            | _ -> raise (new NotImplementedException("No"))
        else
            raise (new NotImplementedException("Non-simple order by is not supported."))

    current :: next

let createInitialQuery (inner: SparqlPattern) =
    { 
        Query = inner |> normalizeSparqlPattern
        Modifiers = [] 
    }

let addModifier (modifier: Modifier) (query: SparqlQuery) =
    let newModifiers = 
        List.append query.Modifiers [modifier]
        |> normalizeModifiers

    { query with Modifiers=newModifiers }

let rec private processSparqlQuery (vdsAlgebra: Query.Algebra.ISparqlAlgebra): SparqlQuery =
    match vdsAlgebra with
    | :? Query.Algebra.Select as orSel ->
        let variables =
            if(orSel.IsSelectAll) then
                orSel.Variables
            else
                orSel.SparqlVariables
                |> Seq.map (fun x -> x.Name)

        orSel.InnerAlgebra 
        |> processSparqlQuery 
        |> addModifier (
            variables 
            |> Seq.map SparqlVariable
            |> List.ofSeq 
            |> Select
        )

    | :? Query.Algebra.IBgp as orBgp ->
        processTriplePatterns orBgp.TriplePatterns |> createInitialQuery

    | :? Query.Algebra.Union as orUnion ->
        seq { orUnion.Lhs; orUnion.Rhs }
        |> Seq.map processSparqlPattern
        |> List.ofSeq
        |> UnionPattern
        |> createInitialQuery

    | :? Query.Algebra.LeftJoin as orLeftJoin ->
        let left = orLeftJoin.Lhs |> processSparqlPattern
        let right = orLeftJoin.Rhs |> processSparqlPattern
        let condition = orLeftJoin.Filter.Expression |> processSparqlCondition
        LeftJoinPattern(left, right, condition) |> createInitialQuery

    | :? Query.Algebra.Filter as orFilter ->
        let inner = orFilter.InnerAlgebra |> processSparqlPattern
        let condition = orFilter.SparqlFilter.Expression |> processSparqlCondition
        FilterPattern(inner, condition) |> createInitialQuery

    | :? Query.Algebra.Join as orJoin ->
        seq { orJoin.Lhs; orJoin.Rhs }
        |> Seq.map processSparqlPattern
        |> List.ofSeq
        |> JoinPattern
        |> createInitialQuery

    | :? Query.Algebra.Extend as orExtend ->
        let inner = orExtend.InnerAlgebra |> processSparqlPattern
        let expression = orExtend.AssignExpression |> processSparqlExpression
        ExtendPattern(inner, [SparqlVariable (orExtend.VariableName), expression]) |> createInitialQuery

    | :? Query.Algebra.OrderBy as orOrderBy ->
        orOrderBy.InnerAlgebra 
        |> processSparqlQuery
        |> addModifier (
            orOrderBy.Ordering
            |> processOrderBy
            |> OrderBy
        )

    | :? Query.Algebra.Slice as orSlice ->
        let limit = if orSlice.Limit <> -1 then Some(orSlice.Limit) else None
        let offset = if orSlice.Offset <> -1 then Some(orSlice.Offset) else None

        orSlice.InnerAlgebra 
        |> processSparqlQuery
        |> addModifier (
            {| Limit = limit; Offset = offset |} |> Slice
        )

    | :? Query.Algebra.Distinct as orDistinct ->
        orDistinct.InnerAlgebra |> processSparqlQuery |> addModifier Distinct

    | _ -> raise (new NotImplementedException(sprintf "The underlying query is not supported: %A" vdsAlgebra))

and private processSparqlPattern (vdsAlgebra: Query.Algebra.ISparqlAlgebra): SparqlPattern =
    match processSparqlQuery vdsAlgebra with
    | { Query = sparqlPattern; Modifiers = [] } -> sparqlPattern
    | x -> raise (new NotSupportedException(sprintf "The inner query is not a supported SPARQL pattern: %A" x))

and private processTriplePatterns (triplePatterns: Query.Patterns.ITriplePattern seq): SparqlPattern =
    let processPattern (orPattern: Query.Patterns.PatternItem) =
        match orPattern with
        | :? Query.Patterns.VariablePattern as orVarPattern ->
            orVarPattern.VariableName |> SparqlVariable |> VariablePattern
        | :? Query.Patterns.NodeMatchPattern as orNodeMatchPattern ->
            orNodeMatchPattern.Node |> processNode |> NodeMatchPattern
        | :? Query.Patterns.BlankNodePattern as orBlankNodePattern ->
            orBlankNodePattern.ID |> BlankNodeVariable |> VariablePattern
        | x -> raise (new NotSupportedException(sprintf "The match pattern is not supported: %A" x))
    
    (EmptyPattern, triplePatterns)
    ||> Seq.fold (
        fun previous current ->
            match current with
            | :? Query.Patterns.TriplePattern as orBgp ->
                let patternMatch = { 
                    Subject = orBgp.Subject |> processPattern
                    Predicate = orBgp.Predicate |> processPattern
                    Object = orBgp.Object |> processPattern
                }

                let notProcessedTriplePattern =
                    patternMatch
                    |> List.singleton
                    |> NotProcessedTriplePatterns
                    |> normalizeSparqlPattern

                JoinPattern [ previous; notProcessedTriplePattern ]
                |> normalizeSparqlPattern

            | :? Query.Patterns.FilterPattern as orFilter ->
                let condition = orFilter.Filter.Expression |> processSparqlCondition
                FilterPattern(previous, condition) |> normalizeSparqlPattern
            | x -> raise (new NotImplementedException(sprintf "The underlying triple pattern is not supported: %A" x))
    )

let private buildDescribeQuery (vdsAlgebra: Query.Algebra.ISparqlAlgebra) (sparqlQuery: SparqlQuery): SparqlQuery =
    match sparqlQuery with
    | { Query = innerPattern; Modifiers = [ Select [variable] ]} ->
        let usedVariables = vdsAlgebra.Variables |> Set.ofSeq
        let rec createVariableName index =
            let variableName = sprintf "_:context-autos%d" index
            if usedVariables |> Set.contains variableName then
                createVariableName (index + 1)
            else
                (SparqlVariable variableName, index)

        let (predicateVariable, foundIndex) = createVariableName 1
        let (objectVariable, _) = createVariableName (foundIndex + 1)

        {
            Query =
                JoinPattern [
                    innerPattern
                    { Subject = VariablePattern variable; Predicate = VariablePattern predicateVariable; Object = VariablePattern objectVariable } |> List.singleton |> NotProcessedTriplePatterns |> normalizeSparqlPattern
                ] |> normalizeSparqlPattern
            Modifiers = [Select [variable; predicateVariable; objectVariable]] |> normalizeModifiers
        }

    | _ ->
        raise (new NotImplementedException(sprintf "Creation of describe query is not supported from the underlying query: %A" sparqlQuery))

let buildSparqlQuery (query: Query.SparqlQuery) =
    match query.QueryType with
    | Query.SparqlQueryType.Ask ->
        raise (new NotImplementedException())
    | Query.SparqlQueryType.Construct ->
        let output = processSparqlQuery (query.ToAlgebra())
        let select = query.ConstructTemplate.Variables |> Seq.map SparqlVariable |> List.ofSeq |> Select
        { output with Modifiers = select :: output.Modifiers |> normalizeModifiers }
    | Query.SparqlQueryType.Describe
    | Query.SparqlQueryType.DescribeAll ->
        let algebra = query.ToAlgebra()
        processSparqlQuery algebra |> buildDescribeQuery algebra
    | Query.SparqlQueryType.Select
    | Query.SparqlQueryType.SelectAll
    | Query.SparqlQueryType.SelectAllDistinct
    | Query.SparqlQueryType.SelectAllReduced
    | Query.SparqlQueryType.SelectDistinct
    | Query.SparqlQueryType.SelectReduced ->
        processSparqlQuery (query.ToAlgebra())
    | otherQueryType ->
        raise (new NotSupportedException(sprintf "The query type %O is not supported" otherQueryType))