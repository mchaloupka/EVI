namespace Slp.Evi.Database

open Slp.Evi.Common.Algebra
open Slp.Evi.Relational.Algebra

type ISqlQuery<'T, 'C> =
    abstract member Query: 'T with get

    abstract member GetColumn: Variable -> 'C

type INamingProvider<'C> =
    interface end

type IExpressionQueryBuilder =
    interface end

type IModifiedQueryBuilder<'T, 'C> =
    abstract member NamingProvider: INamingProvider<'C> with get

    abstract member AddOrdering: IExpressionQueryBuilder * OrderingDirection -> unit

    abstract member CreateQuery: unit -> ISqlQuery<'T, 'C>

type INotModifiedQueryBuilder<'T, 'C> =
    abstract member NamingProvider: INamingProvider<'C> with get

    abstract member ToModifiedQueryBuilder: unit -> IModifiedQueryBuilder<'T, 'C>

type IUnionQueryBuilder<'T, 'C> =
    abstract member NamingProvider: INamingProvider<'C> with get

    abstract member AddUnioned: INotModifiedQueryBuilder<'T, 'C> -> unit

    abstract member CreateQuery: unit -> ISqlQuery<'T, 'C>

type ISqlQueryBuilder<'T, 'C> =
    abstract member CreateNoResultQuery: unit -> ISqlQuery<'T, 'C>

    abstract member CreateSingleEmptyResultQuery: unit -> ISqlQuery<'T, 'C>

    abstract member CreateUnionBuilder: AssignedVariable -> IUnionQueryBuilder<'T, 'C>