namespace Slp.Fsm

open System

type GenericNode private (id: Guid) =
    static member create() = new GenericNode(Guid.NewGuid())

    member _.Identifier = id

    override _.Equals yobj =
        match yobj with
        | :? GenericNode as o -> o.Identifier = id
        | _ -> false

    override _.GetHashCode () = id.GetHashCode ()

    override _.ToString () = sprintf "Node:%A" id

    interface IComparable with
        member _.CompareTo yobj =
            match yobj with
            | :? GenericNode as o -> o.Identifier.CompareTo(id)
            | _ -> invalidArg "yObj" "Cannot compare different types" |> raise

module GenericNode =
    let initialMachine () = FiniteStateMachineBuilder.initial (GenericNode.create ())

    let appendEdge edge = FiniteStateMachineBuilder.appendEdge edge (GenericNode.create ())

    let choiceMachine machines = FiniteStateMachineBuilder.choice (GenericNode.create ()) (GenericNode.create ()) machines

    let transformNode _ = GenericNode.create ()

    let infiniteRepeatMachine machine = FiniteStateMachineBuilder.infiniteRepeat (GenericNode.create ()) (GenericNode.create ()) machine

    let atLeastOneRepeatMachine machine = FiniteStateMachineBuilder.atLeastOneRepeat (GenericNode.create ()) (GenericNode.create ()) transformNode machine

    let repeatMachine count machine = FiniteStateMachineBuilder.repeat transformNode count machine

    let optionalMachine machine = FiniteStateMachineBuilder.optional (GenericNode.create ()) (GenericNode.create()) machine