namespace FiniteStateMachine

open Slp.Fsm
open FsCheck

type ByteBasedEdges =
    | AnyByte
    | ExactByte of byte

type ByteBasedEdgesEvaluator() =
    interface IEdgeEvaluator<ByteBasedEdges, byte> with
        member _.IsOnEdge(edge, x) =
            match edge with
            | AnyByte -> true
            | ExactByte b -> b = x

module ByteBasedEdges =
    type ByteBasedFsm = FiniteStateMachine<GenericNode, ByteBasedEdges>

    let evaluator = ByteBasedEdgesEvaluator ()

    let private machineGenerator =
        let rec sizedEdges edgeCount (nodes: GenericNode list) =
            if edgeCount = 0 then
                { StartStates = Set.empty; EndStates = Set.empty; Edges = Map.empty } |> Gen.constant
            else
                let edgeToAdd =
                    (nodes |> Gen.elements |> Gen.two, Arb.generate<byte option>)
                    ||> Gen.map2 (
                        fun (f, t) e ->
                            match e with
                            | Some(e) -> ExactByte e |> EdgeWithToken, f, t
                            | None -> LambdaEdge, f, t
                    )

                sizedEdges (edgeCount - 1) nodes
                |> Gen.zip edgeToAdd
                |> Gen.filter (
                    fun ((edge, fN, tN), machine) ->
                        match machine.Edges.TryGetValue fN with
                        | true, edges -> edges |> List.exists (fun (t, e) -> e = edge && t = tN) |> not
                        | false, _ -> true
                )
                |> Gen.map (
                    fun ((edge, f, t), machine) ->
                        match edge with
                        | EdgeWithToken e -> machine |> FiniteStateMachine.addEdge e f t
                        | LambdaEdge -> machine |> FiniteStateMachine.addEmptyEdge f t
                )

        let rec withStartNodes (nodes: GenericNode list) startNodesCount (machineGen: Gen<ByteBasedFsm>) =
            if startNodesCount = 0 then
                machineGen
            else
                (withStartNodes nodes (startNodesCount - 1) machineGen, nodes |> Gen.elements)
                ||> Gen.zip
                |> Gen.filter (fun (machine, startNode) -> machine.StartStates |> Set.contains startNode |> not)
                |> Gen.map (
                    fun (machine, startNode) ->
                        { machine with
                            StartStates = machine.StartStates |> Set.add startNode
                        }
                )

        let rec withEndNodes (nodes: GenericNode list) endNodesCount (machineGen: Gen<ByteBasedFsm>) =
            if endNodesCount = 0 then
                machineGen
            else
                (withEndNodes nodes (endNodesCount - 1) machineGen, nodes |> Gen.elements)
                ||> Gen.zip
                |> Gen.filter (fun (machine, endNode) -> machine.EndStates |> Set.contains endNode |> not)
                |> Gen.map (
                    fun (machine, endNode) ->
                        { machine with
                            EndStates = machine.EndStates |> Set.add endNode
                        }
                )

        let sizedMachine operations =
            seq {
                for nodeCount in 0..operations do
                    for startCount in 0..(min (operations - nodeCount) nodeCount) do
                        for endCount in 0..(min (operations - nodeCount - startCount) nodeCount) do
                            let edgeCount = operations - nodeCount - startCount - endCount
                            if edgeCount <= 3 * nodeCount * nodeCount then
                                yield (nodeCount, edgeCount, startCount, endCount) }
            |> Seq.map (
                fun (nodeCount, edgeCount, startCount, endCount) ->
                    let nodes = [ for _ in 1..nodeCount -> GenericNode.create () ]
                    
                    nodes
                    |> sizedEdges edgeCount
                    |> withStartNodes nodes startCount
                    |> withEndNodes nodes endCount
            )
            |> Gen.oneof
            |> Gen.map (FiniteStateMachineBuilder.transformNodes GenericNode.transformNode)

        Gen.sized sizedMachine

    type MachineGenerators =
        static member FiniteStateMachine() =
            { new Arbitrary<ByteBasedFsm>() with
                override _.Generator = machineGenerator
                override _.Shrinker t = Seq.empty
            }

        static member Byte() =
            { new Arbitrary<byte>() with
                override _.Generator = seq { 0; 1 } |> Gen.elements |> Gen.map byte
                override _.Shrinker t = Seq.empty
            }
