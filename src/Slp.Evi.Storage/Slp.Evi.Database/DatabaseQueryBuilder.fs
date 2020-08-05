module Slp.Evi.Database.DatabaseQueryBuilder

open System.Collections.Generic
open Slp.Evi.Relational.Algebra

type private VariableList = HashSet<Variable>

module private VariableList =
    let empty =
        HashSet<Variable>()

    let toList (hashSet: VariableList) =
        hashSet
        |> Seq.toList

    let fromList xs = HashSet<_>(xs |> Seq.ofList)

    let addMany xs (hashSet: VariableList) =
        let r = HashSet<_>(hashSet)
        r.UnionWith(xs |> fromList)
        r

    let union hs2 (hs1: VariableList) =
        let r1 = HashSet<_>(hs1)
        r1.UnionWith(hs2)
        r1

    let intersect hs2 (hs1: VariableList) =
        let r1 = HashSet<_>(hs1)
        r1.IntersectWith(hs2)
        r1

let rec private neededVariablesForExpression expressions result =
    match expressions with
    | [] ->
        result

    | BinaryNumericOperation(_, leftExpr, rightExpr) :: xs ->
        neededVariablesForExpression (leftExpr :: rightExpr :: xs) result

    | Switch statements :: xs ->
        ((xs, result), statements)
        ||> List.fold (
            fun (xst, rt) statement ->
                statement.Expression :: xst, neededVariablesForCondition [ statement.Condition ] rt
        )
        ||> neededVariablesForExpression

    | Coalesce coalesced :: xs ->
        neededVariablesForExpression (coalesced @ xs) result

    | Variable var :: xs ->
        neededVariablesForExpression xs (var :: result)

    | IriSafeVariable var :: xs ->
        neededVariablesForExpression xs (var :: result)

    | Constant _ :: xs ->
        neededVariablesForExpression xs result

    | Concatenation concatenated :: xs ->
        neededVariablesForExpression (concatenated @ xs) result

    | Boolean condition :: xs ->
        neededVariablesForCondition [ condition ] result
        |> neededVariablesForExpression xs

    | Null :: xs ->
        neededVariablesForExpression xs result

and private neededVariablesForCondition conditions result =
    match conditions with
    | [] ->
        result

    | AlwaysFalse :: xs
    | AlwaysTrue :: xs ->
        neededVariablesForCondition xs result

    | Comparison(_, leftExpr, rightExpr) :: xs ->
        result
        |> neededVariablesForExpression [ leftExpr ]
        |> neededVariablesForExpression [ rightExpr ]
        |> neededVariablesForCondition xs

    | Conjunction inners :: xs
    | Disjunction inners :: xs ->
        inners @ xs
        |> neededVariablesForCondition <| result

    | EqualVariableTo(v, _) :: xs ->
        v :: result
        |> neededVariablesForCondition xs

    | EqualVariables(v1, v2) :: xs ->
        v1 :: v2 :: result
        |> neededVariablesForCondition xs

    | IsNull(v) :: xs ->
        v :: result
        |> neededVariablesForCondition xs

    | LanguageMatch(ex1, ex2) :: xs ->
        result
        |> neededVariablesForExpression [ ex1 ]
        |> neededVariablesForExpression [ ex2 ]
        |> neededVariablesForCondition xs

    | Like(ex, _) :: xs ->
        result
        |> neededVariablesForExpression [ ex ]
        |> neededVariablesForCondition xs

    | Not(c) :: xs ->
        c :: xs
        |> neededVariablesForCondition <| result

let rec private neededVariablesForValueBinders valueBinders result =
    match valueBinders with
    | [] ->
        result

    | EmptyValueBinder :: xs ->
        neededVariablesForValueBinders xs result

    | BaseValueBinder(_, vb) :: xs ->
        (result, vb)
        ||> Map.fold (
            fun r _ v ->
                v :: r
        )
        |> neededVariablesForValueBinders xs

    | CoalesceValueBinder coalesced :: xs ->
        coalesced @ xs |> neededVariablesForValueBinders <| result

    | CaseValueBinder(var, cases) :: xs ->
        (xs, cases)
        ||> Map.fold (
            fun xst _ vb ->
                vb :: xst
        )
        |> neededVariablesForValueBinders <| var::result

    | ExpressionValueBinder expressionSet :: xs ->
        [ expressionSet.IsNotErrorCondition ]
        |> neededVariablesForCondition <| result
        |> neededVariablesForExpression [ expressionSet.TypeCategoryExpression ]
        |> neededVariablesForExpression [ expressionSet.TypeExpression ]
        |> neededVariablesForExpression [ expressionSet.BooleanExpression ]
        |> neededVariablesForExpression [ expressionSet.DateTimeExpresion ]
        |> neededVariablesForExpression [ expressionSet.NumericExpression ]
        |> neededVariablesForExpression [ expressionSet.StringExpression ]
        |> neededVariablesForValueBinders xs

let private emptySqlQuery = {
    NamingProvider = NamingProvider.Empty
    Variables = List.empty
    InnerQueries = List.empty
    Limit = None
    Offset = None
    Ordering = List.empty
    IsDistinct = false
}

let rec private translateModel desiredVariables model =
    match model with
    | NoResult ->
        { emptySqlQuery with InnerQueries = NoResultQuery |> List.singleton }

    | SingleEmptyResult ->
        { emptySqlQuery with InnerQueries = SingleEmptyResultQuery |> List.singleton }

    | Modified modifiedModel ->
        let innerResult = translateModel desiredVariables modifiedModel.InnerModel

        if innerResult.IsDistinct || innerResult.Limit.IsSome || innerResult.Offset.IsSome || innerResult.Ordering.IsEmpty |> not then
            sprintf "Another application of modified model on: %A" innerResult
            |> invalidOp
        else
            { innerResult with
                IsDistinct = modifiedModel.IsDistinct
                Limit = modifiedModel.Limit
                Offset = modifiedModel.Offset
                Ordering = modifiedModel.Ordering
            }

    | Union(_, inners) ->
        (inners, (VariableList.empty, List.empty))
        ||> List.foldBack (
            fun notModifiedModel (prevVars, prev) ->
                let (providedVariables, innerResult) = translateNotModifiedModel desiredVariables notModifiedModel
                let selectedVariables = VariableList.intersect desiredVariables providedVariables
                prevVars |> VariableList.union selectedVariables, innerResult :: prev
        )
        |> fun (selectedVariables, innerResults) ->
            let selectedVariablesList = selectedVariables |> VariableList.toList
            let namingProvider = NamingProvider.WithVariables selectedVariablesList
            
            { emptySqlQuery with
                NamingProvider = namingProvider
                Variables = selectedVariablesList
                InnerQueries = innerResults |> List.map SelectQuery
            }

    | NotModified notModifiedModel ->
        let (providedVariables, innerResult) = translateNotModifiedModel desiredVariables notModifiedModel
        let selectedVariables = VariableList.intersect desiredVariables providedVariables
        let selectedVariablesList = selectedVariables |> VariableList.toList
        let namingProvider = NamingProvider.WithVariables selectedVariablesList

        { emptySqlQuery with
            NamingProvider = namingProvider
            Variables = selectedVariablesList
            InnerQueries = innerResult |> SelectQuery |>  List.singleton
        }

and private translateNotModifiedModel desiredVariables notModifiedModel: (VariableList * InnerQuery) =
    let assignedVariables =
        notModifiedModel.Assignments
        |> List.map (fun x -> x.Variable |> Assigned)
        |> HashSet<_>

    let neededVariables =
        desiredVariables
        |> VariableList.toList
        |> neededVariablesForCondition notModifiedModel.Filters
        |> neededVariablesForExpression (notModifiedModel.Assignments |> List.map (fun x -> x.Expression))
        |> neededVariablesForCondition (
            notModifiedModel.Sources 
            |> List.collect (
                function
                | LeftOuterJoinModel(_, condition) -> condition |> List.singleton
                | _ -> List.empty
            )
        )
        |> VariableList.fromList

    ((assignedVariables, { NamingProvider = MergedNamingProvider.Empty; Sources = List.empty; LeftJoinedSources = List.empty; Filters = notModifiedModel.Filters; Assignments = notModifiedModel.Assignments }), notModifiedModel.Sources)
    ||> List.fold (
        fun (resProvidedVariables, resQuery) source ->
            match source with
            | Sql sqlSource ->
                let variables = sqlSource.Columns |> List.map Column
                let namingProvider = NamingProvider.FromTable sqlSource
                let source = (sqlSource, namingProvider) |> InnerTable
                resProvidedVariables |> VariableList.addMany variables, { resQuery with NamingProvider = resQuery.NamingProvider.MergeWith(variables, source); Sources = source :: resQuery.Sources }

            | SubQuery model ->
                let innerResult = translateModel neededVariables model
                let source = innerResult |> InnerSource
                resProvidedVariables |> VariableList.union innerResult.Variables, { resQuery with NamingProvider = resQuery.NamingProvider.MergeWith(innerResult.Variables, source); Sources = source :: resQuery.Sources }

            | LeftOuterJoinModel(model, condition) ->
                let innerResult = translateModel neededVariables (model |> NotModified)
                let source = innerResult |> InnerSource
                resProvidedVariables |> VariableList.union innerResult.Variables, { resQuery with NamingProvider = resQuery.NamingProvider.MergeWith(innerResult.Variables, source); LeftJoinedSources = (source, condition) :: resQuery.LeftJoinedSources }
    )

let translateToQuery boundCalculusModel =
    let neededVariables =
        boundCalculusModel.Bindings
        |> Map.toList
        |> List.map snd
        |> neededVariablesForValueBinders <| List.empty
        |> VariableList.fromList

    translateModel neededVariables boundCalculusModel.Model