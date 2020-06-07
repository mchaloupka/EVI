[<FsCheck.Xunit.Properties(Arbitrary=[| typeof<ByteBasedEdges.MachineGenerators> |], MaxTest = 2000, EndSize = 20)>]
module FiniteStateMachine.Transform

open Slp.Fsm
open FsCheck.Xunit

let accepts = FiniteStateMachine.accepts ByteBasedEdges.evaluator

[<Property>]
let ``RemoveLambdaEdges does not change accept`` (orMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine = orMachine |> FiniteStateMachine.removeLambdaEdges

    (machine |> accepts input) = (orMachine |> accepts input)

[<Property>]
let ``After RemoveLambdaEdges there is no lambda edge`` (orMachine: ByteBasedEdges.ByteBasedFsm) =
    let machine = orMachine |> FiniteStateMachine.removeLambdaEdges

    (true, machine.Edges)
    ||> Map.fold (
        fun current _ edges ->
            (current, edges)
            ||> List.fold (
                fun c (_,e) ->
                    match e with
                    | LambdaEdge -> false
                    | _ -> c
            )
    )