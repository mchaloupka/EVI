namespace Slp.Evi.Database

open System.Collections.Generic
open Slp.Evi.Relational.Algebra

type NamingProvider private (nameMapping: IDictionary<Variable, string>) =

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
    
    static member Empty with get () = new MergedNamingProvider (new Dictionary<_,_>(), new Dictionary<_,_>())

and InnerSource =
    | InnerTable of SqlSource * NamingProvider
    | InnerSource of SqlQuery
    member self.NamingProvider
        with get () =
            match self with
            | InnerTable(_, np) -> np
            | InnerSource q -> q.NamingProvider

and InnerQuery = {
    NamingProvider: MergedNamingProvider
    Sources: InnerSource list
    LeftJoinedSources: (InnerSource * Condition) list
    Filters: Condition list
    Assignments: Assignment list
}

and QueryContent =
    | NoResultQuery
    | SingleEmptyResultQuery
    | SelectQuery of InnerQuery

and SqlQuery = {
    NamingProvider: NamingProvider
    Variables: Variable list
    InnerQueries: QueryContent list
    Limit: int option
    Offset: int option
    Ordering: Ordering list
    IsDistinct: bool
}