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
        let rec splitMachines operations =
            [ for i in 0 .. operations -> i ]
            |> List.map (fun i -> Gen.zip (sizedMachine i) (sizedMachine (operations - i)))
            |> Gen.oneof
        and sizedMachine operations =
            if operations = 0 then
                GenericNode.initialMachine |> Gen.fresh
            else
                let op = operations - 1

                seq {
                    sizedMachine op |> Gen.map(GenericNode.appendEdge AnyByte)
                    (Arb.generate<byte>, sizedMachine op) ||> Gen.map2 (fun b -> GenericNode.appendEdge (ExactByte b))
                    splitMachines op |> Gen.map (fun (l, r) -> FiniteStateMachineBuilder.appendMachine l r)
                    sizedMachine op |> Gen.map FiniteStateMachineBuilder.infiniteRepeat
                    splitMachines op |> Gen.map (fun (l, r) -> [l; r] |> GenericNode.choiceMachine)
                } |> Gen.oneof

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
