namespace Slp.Fsm

module FiniteStateMachineBuilder =
    let initial node = { StartStates = set [ node ]; Edges = Map.empty; EndStates = set [ node ] }

    let appendEdge edge node appendTo =
        ({ appendTo with
            EndStates = set [ node ]
        }, appendTo.EndStates)
        ||> Set.fold (
            fun current endState ->
                FiniteStateMachine.addEdge edge endState node current
        )

    let private interconnect fromNodes toNodes machine =
        (machine, fromNodes)
        ||> Seq.fold (
            fun current fromNode ->
                (current, toNodes)
                ||> Seq.fold (
                    fun c toNode -> 
                        c |> FiniteStateMachine.addEmptyEdge fromNode toNode
                )
        )

    let private addNewEndState node machine =
        { machine with
            EndStates = set [ node ]
        }
        |> interconnect machine.EndStates [ node ]

    let private addNewStartState node machine =
        { machine with
            StartStates = set [ node ]
        }
        |> interconnect [ node ] machine.StartStates

    let appendMachine machine appendTo =
        { appendTo with
            Edges = FiniteStateMachine.mergeEdges machine.Edges appendTo.Edges
            EndStates = machine.EndStates
        }
        |> interconnect appendTo.EndStates machine.StartStates

    let optional newStartState newEndState machine =
        machine
        |> addNewStartState newStartState
        |> addNewEndState newEndState
        |> fun m -> m |> interconnect m.StartStates m.EndStates

    let transformNodes transformNode machine =
        let nodeTransform =
            (machine.EndStates |> Set.union machine.StartStates, machine.Edges)
            ||> Map.fold (
                fun current node edges ->
                    (current |> Set.add node, edges)
                    ||> List.fold (
                        fun cur (n, _) ->
                            cur |> Set.add n
                    )
            )
            |> Set.toSeq
            |> Seq.map (fun x -> x, transformNode x)
            |> Map.ofSeq

        let transformedEdges =
            (Map.empty, machine.Edges)
            ||> Map.fold (
                fun current orNode orEdges ->
                    let edges = orEdges |> List.map (fun (n, e) -> nodeTransform.[n], e)
                    let node = nodeTransform.[orNode]
                    current |> Map.add node edges
            )

        {
            StartStates = machine.StartStates |> Set.map (fun n -> nodeTransform.[n])
            EndStates = machine.EndStates |> Set.map (fun n -> nodeTransform.[n])
            Edges = transformedEdges
        }

    let infiniteRepeat newStartState newEndState machine =
        machine
        |> optional newStartState newEndState
        |> FiniteStateMachine.addEmptyEdge newEndState newStartState

    let atLeastOneRepeat newStartState newIntermediateState transformNode machine =
        machine
        |> infiniteRepeat newStartState newIntermediateState
        |> appendMachine (machine |> transformNodes transformNode)

    let repeat transformNode count machine =
        if count < 2 then
            "Count needs to be at least 2" |> invalidArg "count" |> raise

        let rec repeatImpl appendCount current =
            if appendCount = 0 then
                current
            else
                current
                |> appendMachine (machine |> transformNodes transformNode)
                |> repeatImpl (appendCount - 1)

        repeatImpl (count - 1) machine

    let choice newStartNode newEndState machines =
        ({ initial newStartNode with
            EndStates = set [ newEndState ]
        }, machines)
        ||> List.fold (
            fun current machine ->
                { current with
                    Edges = FiniteStateMachine.mergeEdges current.Edges machine.Edges
                }
                |> interconnect current.StartStates machine.StartStates
                |> interconnect machine.EndStates current.EndStates
        )