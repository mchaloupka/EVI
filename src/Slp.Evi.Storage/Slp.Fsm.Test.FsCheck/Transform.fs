[<FsCheck.Xunit.Properties(Arbitrary=[| typeof<ByteBasedEdges.MachineGenerators> |], MaxTest = 2000, EndSize = 30)>]
module FiniteStateMachine.Transform

open Slp.Fsm
open FsCheck.Xunit

let accepts = FiniteStateMachine.accepts ByteBasedEdges.edgeEvaluator

[<Property>]
let ``RemoveLambdaEdges does not change accept`` (orMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine = orMachine |> FiniteStateMachine.removeLambdaEdges

    (machine |> accepts input) = (orMachine |> accepts input)

[<Property(EndSize=15)>]
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

[<Property(EndSize=15)>]
let ``After RemoveNonReachable the accept is unchanged`` (orMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine = orMachine |> FiniteStateMachine.removeNonReachable

    (machine |> accepts input) = (orMachine |> accepts input)

[<Property(EndSize=15)>]
let ``After RemoveNonReachable the non-connected to end-states machine is empty`` (orMachine: ByteBasedEdges.ByteBasedFsm) =
    let machine = 
        { orMachine with
            EndStates = set [ GenericNode.create () ]
        } |> FiniteStateMachine.removeNonReachable

    machine = {
        StartStates = Set.empty
        EndStates = Set.empty
        Edges = Map.empty
    }

[<Property(EndSize=15)>]
let ``After RemoveNonReachable the non-connected to start-states machine is empty`` (orMachine: ByteBasedEdges.ByteBasedFsm) =
    let machine = 
        { orMachine with
            StartStates = set [ GenericNode.create () ]
        } |> FiniteStateMachine.removeNonReachable

    machine = {
        StartStates = Set.empty
        EndStates = Set.empty
        Edges = Map.empty
    }

[<Property>]
let ``After RemoveNonReachable the non-connected is empty`` (leftMachine: ByteBasedEdges.ByteBasedFsm) (rightMachine: ByteBasedEdges.ByteBasedFsm) =
    let machine =
        [
            { leftMachine with
                StartStates = set [ GenericNode.create () ]
            }
            { rightMachine with
                EndStates = set [ GenericNode.create () ]
            }
        ]
        |> GenericNode.choiceMachine
        |> FiniteStateMachine.removeNonReachable

    machine = {
        StartStates = Set.empty
        EndStates = Set.empty
        Edges = Map.empty
    }

[<Property>]
let ``Intersection of machines`` (leftMachine: ByteBasedEdges.ByteBasedFsm) (rightMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine =
        leftMachine
        |> FiniteStateMachine.intersect rightMachine

    (machine |> accepts input) = ((leftMachine |> accepts input) && (rightMachine |> accepts input))