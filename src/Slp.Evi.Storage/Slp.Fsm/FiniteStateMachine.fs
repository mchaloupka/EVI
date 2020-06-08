namespace Slp.Fsm

type Edge<'C> =
    | LambdaEdge
    | EdgeWithToken of 'C

type FiniteStateMachine<'N, 'C> when 'N : comparison =
    { StartStates: Set<'N>; Edges: Map<'N, ('N * Edge<'C>) list>; EndStates: Set<'N> }

module FiniteStateMachine =
    let internal mergeEdges (leftEdges: Map<_, _ list>) (rightEdges: Map<_, _ list>) =
        (leftEdges, rightEdges)
        ||> Map.fold (
            fun current from edges ->
                match current.TryGetValue from with
                | true, prevEdges -> current |> Map.add from ((edges @ prevEdges) |> List.distinct)
                | false, _ -> current |> Map.add from edges
        )

    let removeEdge fromNode toNode edge machine =
        match machine.Edges.TryGetValue fromNode with
        | true, edges ->
            let newFromNodeEdges =
                edges
                |> List.filter (fun (t, e) -> t <> toNode || e <> edge)

            { machine with
                Edges =
                    if newFromNodeEdges |> List.isEmpty then
                        machine.Edges |> Map.remove fromNode
                    else
                        machine.Edges |> Map.add fromNode newFromNodeEdges
            }

        | false, _ ->
            machine

    let private addAnyEdge edgeToAdd fromNode toNode appendTo =
        let updatedFromNode =
            match appendTo.Edges.TryGetValue fromNode with
            | true, edges -> (toNode, edgeToAdd) :: edges |> List.distinct
            | false, _ -> [ (toNode, edgeToAdd) ]

        { appendTo with
            Edges = appendTo.Edges |> Map.add fromNode updatedFromNode
        }

    let addEmptyEdge fromNode toNode =
        addAnyEdge LambdaEdge fromNode toNode

    let addEdge edge fromNode toNode =
        let edgeToAdd = EdgeWithToken edge
        addAnyEdge edgeToAdd fromNode toNode

    let accepts (edgeEvaluator: ('C * 'X) -> bool) input machine =
        let rec evaluateQueue stepsQueue =
            match stepsQueue with
            | [] ->
                false
            | ([], state, _) :: _ when machine.EndStates |> Set.contains state ->
                true
            | (inp, state, history) :: cs ->
                let nextStates =
                    match machine.Edges.TryGetValue state with
                    | true, edges -> edges
                    | false, _ -> List.empty
                    |> List.collect (
                        fun (node, edge) ->
                            match edge, inp with
                            | LambdaEdge, _ when history |> Set.contains (node, inp) |> not ->
                                (inp, node, history |> Set.add (node, inp)) |> List.singleton
                            | EdgeWithToken c, x::xs when edgeEvaluator(c, x) && history |> Set.contains (node, xs) |> not ->
                                (xs, node, history |> Set.add (node, xs)) |> List.singleton
                            | _ ->
                                List.empty
                    )

                evaluateQueue (cs @ nextStates)

        machine.StartStates
        |> Set.toList
        |> List.map (fun s -> input, s, Set.empty)
        |> evaluateQueue

    let removeLambdaEdges machine =
        let rec removeLambdaEdgesImpl removed machine =
            let lambdaEdges =
                machine.Edges
                |> Seq.collect (
                    fun x ->
                        x.Value |> Seq.map (fun (toNode, edge) -> x.Key, toNode, edge)
                )
                |> Seq.filter (
                    function
                    | _, _, LambdaEdge -> true
                    | _ -> false
                )
                |> Seq.map (fun (fromNode, toNode, _) -> fromNode, toNode)

            if lambdaEdges |> Seq.isEmpty then
                machine
            else
                let (fromNode, toNode) = lambdaEdges |> Seq.head

                if fromNode = toNode then
                    machine
                    |> removeEdge fromNode toNode LambdaEdge
                    |> removeLambdaEdgesImpl removed
                else
                    let newStartNodes =
                        if machine.StartStates |> Set.contains fromNode then
                            machine.StartStates |> Set.add toNode
                        else
                            machine.StartStates

                    let newEndNodes =
                        if machine.EndStates |> Set.contains toNode then
                            machine.EndStates |> Set.add fromNode
                        else
                            machine.EndStates

                    let toAddEdges =
                        match machine.Edges.TryGetValue toNode with
                        | true, edges ->
                            edges |> List.map (fun (t, e) -> fromNode, t, e)
                        | false, _ -> List.empty
                        |> List.filter (
                            function
                            | f, t, LambdaEdge -> not <| (removed |> Set.contains (f, t) || (f = fromNode && t = toNode) || (f = t))
                            | _ -> true
                        )

                    ({ machine with
                        StartStates = newStartNodes
                        EndStates = newEndNodes
                    }, toAddEdges)
                    ||> List.fold (
                        fun current (fN, tN, e) ->
                            current
                            |> addAnyEdge e fN tN
                    )
                    |> removeEdge fromNode toNode LambdaEdge
                    |> removeLambdaEdgesImpl (removed |> Set.add (fromNode, toNode))

        removeLambdaEdgesImpl Set.empty machine

    let removeNonReachable machine =
        let rec reachableFrom (edgesMap:Map<'X,'X list>) reachable toProcess =
            match toProcess with
            | [] -> reachable
            | x :: xs ->
                if reachable |> Set.contains x then
                    reachableFrom edgesMap reachable xs
                else
                    let newReachable = reachable |> Set.add x
                    let addToProcess =
                        match edgesMap.TryGetValue x with
                        | true, edges -> edges |> List.distinct
                        | false, _ -> List.empty

                    reachableFrom edgesMap newReachable (xs @ addToProcess)

        let reachableFromStart =
            let edgesMap =
                machine.Edges
                |> Map.map (
                    fun key value ->
                        value |> List.map fst |> List.distinct
                )

            reachableFrom edgesMap Set.empty (machine.StartStates |> Set.toList)

        let reachableFromEnd =
            let invertedEdgesMap =
                machine.Edges
                |> Map.toList
                |> List.collect (
                    fun (from, edges) ->
                        edges |> List.map fst |> List.map (fun x -> x, from)
                )
                |> List.groupBy fst
                |> List.map (fun (key, value) -> key, value |> List.map snd |> List.distinct)
                |> Map.ofList

            reachableFrom invertedEdgesMap Set.empty (machine.EndStates |> Set.toList)

        let reachableStates = Set.intersect reachableFromStart reachableFromEnd

        let newStartStates = Set.intersect reachableStates machine.StartStates
        let newEndStates = Set.intersect reachableStates machine.EndStates
        let newEdges =
            (Map.empty, machine.Edges)
            ||> Map.fold (
                fun current from fromEdges ->
                    if reachableStates |> Set.contains from then
                        let newFromEdges =
                            fromEdges
                            |> List.filter (fst >> (fun x -> reachableStates |> Set.contains x))

                        if newFromEdges |> List.isEmpty |> not then
                            current |> Map.add from newFromEdges
                        else
                            current
                    else
                        current
            )

        {
            StartStates = newStartStates
            EndStates = newEndStates
            Edges = newEdges
        }

    let intersect leftMachine rightMachine =
        leftMachine