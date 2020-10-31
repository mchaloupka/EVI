module Slp.Evi.Storage.Core.Database.DatabaseQueryBuilder

open System.Collections.Generic
open Slp.Evi.Storage.Core.Relational.Algebra
open Slp.Evi.Storage.Core.Common.Database

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

    | ConditionedValueBinder (condition, valueBinder) :: xs ->
        [ condition ]
        |> neededVariablesForCondition <| result
        |> neededVariablesForValueBinders (valueBinder :: xs)

let private emptySqlQuery = {
    NamingProvider = NamingProvider.Empty
    Variables = List.empty
    InnerQueries = List.empty
    Limit = None
    Offset = None
    Ordering = List.empty
    IsDistinct = false
}

type private Builder(databaseSchema: ISqlDatabaseSchema) =
    let addTypeToExpressionContent contentType content =
        {
            ProvidedType = contentType
            ActualType = contentType
            Expression = content
        }

    let rec transformCondition = function
        | Condition.AlwaysTrue -> TypedCondition.AlwaysTrue
        | Condition.AlwaysFalse -> TypedCondition.AlwaysFalse
        | Condition.Comparison (c, le, re) ->
            let transformedLe = le |> transformExpression
            let transformedRe = re |> transformExpression
            let commonType = databaseSchema.GetCommonType(transformedLe.ProvidedType, transformedRe.ProvidedType)
            TypedCondition.Comparison (c, { transformedLe with ProvidedType = commonType }, { transformedRe with ProvidedType = commonType })

        | Condition.Conjunction cs -> cs |> List.map transformCondition |> TypedCondition.Conjunction
        | Condition.Disjunction cs -> cs |> List.map transformCondition |> TypedCondition.Disjunction
        | Condition.EqualVariableTo (v, l) -> TypedCondition.EqualVariableTo (v, l)
        | Condition.EqualVariables (vl, vr) -> TypedCondition.EqualVariables (vl, vr)
        | Condition.IsNull v -> v |> TypedCondition.IsNull
        | Condition.LanguageMatch (lang, langRange) -> TypedCondition.LanguageMatch (lang |> transformExpression, langRange |> transformExpression)
        | Condition.Like (expr, pattern) -> TypedCondition.Like (expr |> transformExpression, pattern)
        | Condition.Not c -> c |> transformCondition |> TypedCondition.Not

    and transformExpression expression: TypedExpression =
        match expression with
        | Expression.BinaryNumericOperation (op, le, re) ->
            let transformedLe = le |> transformExpression
            let transformedRe = re |> transformExpression
            let commonType = databaseSchema.GetCommonType(transformedLe.ProvidedType, transformedRe.ProvidedType)
            
            TypedExpressionContent.BinaryNumericOperation (op, { transformedLe with ProvidedType = commonType }, { transformedRe with ProvidedType = commonType })
            |> addTypeToExpressionContent commonType

        | Expression.Switch es ->
            ((databaseSchema.NullType, List.empty), es)
            ||> List.fold (
                fun (t, ex) case ->
                    let transformedE = case.Expression |> transformExpression
                    let transformedC = case.Condition |> transformCondition
                    let commonType = databaseSchema.GetCommonType(t, transformedE.ProvidedType)
                    commonType, (transformedC, transformedE) :: ex
            )
            |> fun (ct, ex) ->
                ex
                |> List.map (fun (c, e) -> { TypedCaseStatement.Condition = c; TypedCaseStatement.Expression = { e with ProvidedType = ct } })
                |> List.rev
                |> TypedExpressionContent.Switch
                |> addTypeToExpressionContent ct

        | Expression.Coalesce es ->
            ((databaseSchema.NullType, List.empty), es)
            ||> List.fold (
                fun (t, ex) e ->
                    let transformedE = e |> transformExpression
                    let commonType = databaseSchema.GetCommonType(t, transformedE.ProvidedType)
                    commonType, transformedE :: ex
            )
            |> fun (ct, ex) ->
                ex
                |> List.map (fun e -> { e with ProvidedType = ct })
                |> TypedExpressionContent.Coalesce
                |> addTypeToExpressionContent ct

        | Expression.Variable (Assigned v) ->
            TypedExpressionContent.Variable (Assigned v) |> addTypeToExpressionContent v.SqlType
        | Expression.Variable (Column c) ->
            TypedExpressionContent.Variable (Column c) |> addTypeToExpressionContent c.Schema.SqlType
        | Expression.IriSafeVariable (Assigned v) ->
            TypedExpressionContent.IriSafeVariable (Assigned v) |> addTypeToExpressionContent v.SqlType
        | Expression.IriSafeVariable (Column c) ->
            TypedExpressionContent.IriSafeVariable (Column c) |> addTypeToExpressionContent databaseSchema.StringType
        | Expression.Constant l ->
            let t =
                match l with
                | String _ -> databaseSchema.StringType
                | Int _ -> databaseSchema.IntegerType
                | Double _ -> databaseSchema.DoubleType
                | DateTimeLiteral _ -> databaseSchema.DateTimeType

            TypedExpressionContent.Constant l
            |> addTypeToExpressionContent t

        | Expression.Concatenation es ->
            es
            |> List.map (
                transformExpression >> (
                    fun e ->
                        { e with ProvidedType = databaseSchema.StringType }
                )
            )
            |> TypedExpressionContent.Concatenation
            |> addTypeToExpressionContent databaseSchema.StringType

        | Expression.Boolean c ->
            c
            |> transformCondition
            |> TypedExpressionContent.Boolean
            |> addTypeToExpressionContent databaseSchema.BooleanType

        | Expression.Null ->
            TypedExpressionContent.Null
            |> addTypeToExpressionContent databaseSchema.NullType

    let rec translateModel desiredVariables model =
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
                    Ordering =
                        modifiedModel.Ordering
                        |> List.map (
                            fun x ->
                                { TypedOrdering.Expression = x.Expression |> transformExpression; TypedOrdering.Direction = x.Direction }
                        )
                }

        | Union(_, inners) ->
            (inners, (VariableList.empty, List.empty))
            ||> List.foldBack (
                fun notModifiedModel (prevVars, prev) ->
                    let innerResult = translateNotModifiedModel desiredVariables notModifiedModel
                    let selectedVariables = VariableList.intersect desiredVariables innerResult.ProvidedVariables
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
            let innerResult = translateNotModifiedModel desiredVariables notModifiedModel
            let selectedVariables = VariableList.intersect desiredVariables innerResult.ProvidedVariables
            let selectedVariablesList = selectedVariables |> VariableList.toList
            let namingProvider = NamingProvider.WithVariables selectedVariablesList

            { emptySqlQuery with
                NamingProvider = namingProvider
                Variables = selectedVariablesList
                InnerQueries = innerResult |> SelectQuery |>  List.singleton
            }

    and translateNotModifiedModel desiredVariables notModifiedModel =
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

        ({
            NamingProvider = MergedNamingProvider.Empty
            ProvidedVariables = assignedVariables
            Sources = List.empty
            LeftJoinedSources = List.empty
            Filters =
                notModifiedModel.Filters
                |> List.map transformCondition
            Assignments =
                notModifiedModel.Assignments
                |> List.map (
                    fun x ->
                        { TypedAssignment.Variable = x.Variable; TypedAssignment.Expression = x.Expression |> transformExpression }
                )
        }, notModifiedModel.Sources)
        ||> List.fold (
            fun resQuery source ->
                match source with
                | Sql sqlSource ->
                    let variables = sqlSource.Columns |> List.map Column
                    let namingProvider = NamingProvider.FromTable sqlSource
                    let source = (sqlSource, namingProvider) |> InnerTable
                    { resQuery with
                        NamingProvider = resQuery.NamingProvider.MergeWith(variables, source)
                        Sources = source :: resQuery.Sources
                        ProvidedVariables = resQuery.ProvidedVariables |> VariableList.addMany variables
                    }

                | SubQuery model ->
                    let innerResult = translateModel neededVariables model
                    let source = innerResult |> InnerSource
                    { resQuery with
                        NamingProvider = resQuery.NamingProvider.MergeWith(innerResult.Variables, source)
                        Sources = source :: resQuery.Sources
                        ProvidedVariables = resQuery.ProvidedVariables |> VariableList.union innerResult.Variables
                    }

                | LeftOuterJoinModel(model, condition) ->
                    let innerResult = translateModel neededVariables (model |> NotModified)
                    let source = innerResult |> InnerSource
                    { resQuery with
                        NamingProvider = resQuery.NamingProvider.MergeWith(innerResult.Variables, source)
                        LeftJoinedSources = (source, condition |> transformCondition) :: resQuery.LeftJoinedSources
                        ProvidedVariables = resQuery.ProvidedVariables |> VariableList.union innerResult.Variables
                    }
        )

    [<CompiledName("Build")>]
    member _.build = translateModel

let translateToQuery databaseSchema boundCalculusModel =
    let builder = Builder(databaseSchema)

    let neededVariables =
        boundCalculusModel.Bindings
        |> Map.toList
        |> List.map snd
        |> neededVariablesForValueBinders <| List.empty
        |> VariableList.fromList

    builder.build neededVariables boundCalculusModel.Model
