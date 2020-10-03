namespace Slp.Evi.Storage.Core.Database

open System.Collections.Generic
open Slp.Evi.Storage.Core.Relational.Algebra
open System.Runtime.InteropServices
open Slp.Evi.Storage.Core.Common.Algebra
open Slp.Evi.Storage.Core.Common.Database

type NamingProvider private (nameMapping: IDictionary<Variable, string>) =
    member _.TryGetVariableName(var: Variable, [<Out>] value: byref<string>) =
        nameMapping.TryGetValue(var, &value)

    static member Empty with get () = new Dictionary<_,_>() |> NamingProvider
    static member WithVariables variables = variables |> List.mapi (fun i v -> v, sprintf "c%d" i) |> dict |> NamingProvider
    static member FromTable table = table.Columns |> List.map (fun c -> c |> Column, c.Schema.Name) |> dict |> NamingProvider
    
and MergedNamingProvider private (variablesMapping: IDictionary<Variable, InnerSource>, sourceNaming: IDictionary<InnerSource, string>) =
    member _.MergeWith(variables: Variable list, source: InnerSource) =
        let newVariablesMapping = new Dictionary<Variable, InnerSource>(variablesMapping)
        variables |> List.iter (fun v -> newVariablesMapping.Add(v, source))

        let newSourceNamings = new Dictionary<InnerSource, string>(sourceNaming)
        newSourceNamings.Add(source, sprintf "s%d" (newSourceNamings.Count + 1))

        new MergedNamingProvider(newVariablesMapping, newSourceNamings)

    member _.TryGetSource(var: Variable, [<Out>] value: byref<InnerSource>) = variablesMapping.TryGetValue(var, &value)

    member _.TryGetSourceName(source: InnerSource, [<Out>] value: byref<string>) = sourceNaming.TryGetValue(source, &value)
    
    static member Empty with get () = new MergedNamingProvider (new Dictionary<_,_>(), new Dictionary<_,_>())

and InnerSource =
    | InnerTable of SqlSource * NamingProvider
    | InnerSource of SqlQuery
    member self.NamingProvider
        with get () =
            match self with
            | InnerTable(_, np) -> np
            | InnerSource q -> q.NamingProvider

and TypedCondition =
    | AlwaysFalse
    | AlwaysTrue
    | Comparison of Comparisons * TypedExpression * TypedExpression
    | Conjunction of TypedCondition list
    | Disjunction of TypedCondition list
    | EqualVariableTo of Variable * Literal
    | EqualVariables of Variable * Variable
    | IsNull of Variable
    | LanguageMatch of TypedExpression * TypedExpression
    | Like of TypedExpression * string
    | Not of TypedCondition

and TypedCaseStatement = { Condition: TypedCondition; Expression: TypedExpression }

and TypedExpressionContent =
    | BinaryNumericOperation of ArithmeticOperator * TypedExpression * TypedExpression
    | Switch of TypedCaseStatement list
    | Coalesce of TypedExpression list
    | Variable of Variable
    | IriSafeVariable of Variable
    | Constant of Literal
    | Concatenation of TypedExpression list
    | Boolean of TypedCondition
    | Null

and TypedExpression = {
    ProvidedType: ISqlColumnType
    ActualType: ISqlColumnType
    Expression: TypedExpressionContent
}

and TypedAssignment = { Variable: AssignedVariable; Expression: TypedExpression }

and InnerQuery = {
    ProvidedVariables: HashSet<Variable>
    NamingProvider: MergedNamingProvider
    Sources: InnerSource list
    LeftJoinedSources: (InnerSource * TypedCondition) list
    Filters: TypedCondition list
    Assignments: TypedAssignment list
}

and QueryContent =
    | NoResultQuery
    | SingleEmptyResultQuery
    | SelectQuery of InnerQuery

and TypedOrdering = { Expression: TypedExpression; Direction: OrderingDirection }

and SqlQuery = {
    NamingProvider: NamingProvider
    Variables: Variable list
    InnerQueries: QueryContent list
    Limit: int option
    Offset: int option
    Ordering: TypedOrdering list
    IsDistinct: bool
}