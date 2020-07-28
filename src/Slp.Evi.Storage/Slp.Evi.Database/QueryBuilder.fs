module Slp.Evi.Database.QueryBuilder

open Slp.Evi.Relational.Algebra

let translateModel (sqlQueryBuilder: ISqlQueryBuilder<_,_>) =
    let rec processExpression namingProvider expression =
        invalidOp "n/a"

    let rec processNotModified maybeNamingProvider model: INotModifiedQueryBuilder<_, _> =
        invalidOp "n/a"
    
    and processModel maybeNamingProvider model =
        match model with
        | NoResult ->
            sqlQueryBuilder.CreateNoResultQuery ()

        | SingleEmptyResult ->
            sqlQueryBuilder.CreateSingleEmptyResultQuery ()

        | Union(assignedVariable, unioned) ->
            let unionBuilder = sqlQueryBuilder.CreateUnionBuilder assignedVariable
            let namingProvider = unionBuilder.NamingProvider |> Some
            unioned
            |> List.map (processNotModified namingProvider)
            |> List.iter unionBuilder.AddUnioned
            unionBuilder.CreateQuery ()

        | Modified modifiedModel ->
            let inner = modifiedModel.InnerModel |> processNotModified maybeNamingProvider
            let modified = inner.ToModifiedQueryBuilder ()
            let namingProvider = modified.NamingProvider

            modifiedModel.Ordering
            |> List.iter (
                fun orderingPart ->
                    let expr = orderingPart.Expression |> processExpression namingProvider
                    modified.AddOrdering(expr, orderingPart.Direction)
            )

            invalidOp "Other bits"

            modified.CreateQuery ()


    processModel None