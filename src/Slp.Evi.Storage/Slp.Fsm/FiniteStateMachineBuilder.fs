namespace Slp.Fsm

module FiniteStateMachineBuilder =
    let initial node = { StartState = node; Edges = Map.empty; EndStates = set [ node ] }

    let private addAnyEdge edgeToAdd fromNode toNode appendTo =
        let updatedFromNode =
            match appendTo.Edges.TryGetValue fromNode with
            | true, edges -> (toNode, edgeToAdd) :: edges
            | false, _ -> [ (toNode, edgeToAdd) ]

        { appendTo with
            Edges = appendTo.Edges |> Map.add fromNode updatedFromNode
        }

    let addEmptyEdge fromNode toNode =
        addAnyEdge Empty fromNode toNode

    let addEdge edge fromNode toNode =
        let edgeToAdd = WithToken edge
        addAnyEdge edgeToAdd fromNode toNode

    let appendEdge edge node appendTo =
        ({ appendTo with
            EndStates = set [ node ]
        }, appendTo.EndStates)
        ||> Set.fold (
            fun current endState ->
                addEdge edge endState node current
        )

    let private mergeEdges (leftEdges: Map<_, _ list>) (rightEdges: Map<_, _ list>) =
        (leftEdges, rightEdges)
        ||> Map.fold (
            fun current from edges ->
                match current.TryGetValue from with
                | true, prevEdges -> current |> Map.add from (edges @ prevEdges)
                | false, _ -> current |> Map.add from edges
        )

    let appendMachine machine appendTo =
        ({ appendTo with
            Edges = mergeEdges machine.Edges appendTo.Edges
            EndStates = machine.EndStates
        }, appendTo.EndStates)
        ||> Set.fold (
            fun current endState ->
                current |> addEmptyEdge endState machine.StartState
        )

    let optional machine =
        (machine, machine.EndStates)
        ||> Set.fold (
            fun current endState ->
                addEmptyEdge machine.StartState endState current
        )

    let transformNodes transformNode machine =
        let nodeTransform =
            (machine.EndStates |> Set.add machine.StartState, machine.Edges)
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
            StartState = nodeTransform.[machine.StartState]
            EndStates = machine.EndStates |> Set.map (fun n -> nodeTransform.[n])
            Edges = transformedEdges
        }

    let infiniteRepeat machine =
        ({ machine with
            EndStates = machine.EndStates |> Set.add machine.StartState
        }, machine.EndStates)
        ||> Set.fold (
            fun current endState ->
                addEmptyEdge endState machine.StartState current
        )

    let atLeastOneRepeat transformNode machine =
        machine
        |> infiniteRepeat
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

    let choice newStartNode machines =
        (initial newStartNode, machines)
        ||> List.fold (
            fun current machine ->
                { current with
                    Edges = mergeEdges current.Edges machine.Edges
                    EndStates = current.EndStates |> Set.union machine.EndStates
                }
                |> addEmptyEdge current.StartState machine.StartState
        )