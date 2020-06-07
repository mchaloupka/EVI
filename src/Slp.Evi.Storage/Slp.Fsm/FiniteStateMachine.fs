namespace Slp.Fsm

type Edge<'C> =
    | LambdaEdge
    | EdgeWithToken of 'C

type FiniteStateMachine<'N, 'C> when 'N : comparison =
    { StartStates: Set<'N>; Edges: Map<'N, ('N * Edge<'C>) list>; EndStates: Set<'N> }

type IEdgeEvaluator<'C, 'X> =
    abstract member IsOnEdge: 'C * 'X -> bool

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

    let accepts (evaluator: IEdgeEvaluator<'C, 'X>) input machine =
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
                            | EdgeWithToken c, x::xs when evaluator.IsOnEdge(c, x) && history |> Set.contains (node, xs) |> not ->
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
