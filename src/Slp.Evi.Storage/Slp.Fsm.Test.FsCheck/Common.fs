namespace FiniteStateMachine

open Slp.Fsm
open FsCheck

type ByteBasedEdges =
    | AnyByte
    | ExactByte of byte

module ByteBasedEdges =
    type ByteBasedFsm = FiniteStateMachine<GenericNode, ByteBasedEdges>

    let edgeEvaluator (edge, x) =
        match edge with
        | AnyByte -> true
        | ExactByte b -> b = x

    let private machineGenerator =
        let cartesian xs ys = 
            xs |> List.collect (fun x -> ys |> List.map (fun y -> x, y))

        let tuples xs = cartesian xs xs

        let setsOfSize n input =
            let result =
                ([ Set.empty ], input)
                ||> List.fold (
                    fun results i ->
                        results
                        |> List.collect (
                            fun result ->
                                if result |> Set.count = n then
                                    [ result ]
                                else
                                    [ result; result |> Set.add i ]
                        )
                )
                |> List.filter (fun x -> x |> Set.count = n)

            if result |> List.isEmpty then
                sprintf "Cannot create a unique set of size %d from %A" n input |> invalidArg "n" |> raise
            else
                result

        let sizedEdges edgeCount (nodes: GenericNode list) =
            tuples nodes
            |> cartesian [ None; Some(0uy); Some(1uy) ]
            |> setsOfSize edgeCount
            |> Gen.elements
            |> Gen.map (
                fun edges ->
                    ({ StartStates = Set.empty; EndStates = Set.empty; Edges = Map.empty }, edges)
                    ||> Set.fold (
                        fun machine edge ->
                            match edge with
                            | None, (f, t) -> machine |> FiniteStateMachine.addEmptyEdge f t
                            | Some(b), (f, t) -> machine |> FiniteStateMachine.addEdge (ExactByte(b)) f t
                    )
            )

        let withStartNodes (nodes: GenericNode list) startNodesCount (machineGen: Gen<ByteBasedFsm>) =
            nodes
            |> setsOfSize startNodesCount
            |> Gen.elements
            |> Gen.zip machineGen
            |> Gen.map (
                fun (machine, startStates) ->
                    { machine with
                        StartStates = startStates
                    }
            )

        let rec withEndNodes (nodes: GenericNode list) endNodesCount (machineGen: Gen<ByteBasedFsm>) =
            nodes
            |> setsOfSize endNodesCount
            |> Gen.elements
            |> Gen.zip machineGen
            |> Gen.map (
                fun (machine, endStates) ->
                    { machine with
                        EndStates = endStates
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

        Gen.sized sizedMachine |> Gen.scaleSize (fun x -> x / 3)

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
