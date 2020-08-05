module Slp.Evi.Storage.Core.ValueBinderLoader

open VDS.RDF
open Slp.Evi.Sparql.Algebra

let getVariableName =
    function
    | SparqlVariable x -> x
    | BlankNodeVariable x -> x

let loadValue rdfHandler namingProvider row valueBinder: INode option =
    "not implemented" |> invalidOp