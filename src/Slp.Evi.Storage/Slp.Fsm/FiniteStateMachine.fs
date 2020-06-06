namespace Slp.Fsm

type Edge<'C> =
    | Empty
    | WithToken of 'C

type FiniteStateMachine<'N, 'C> when 'N : comparison =
    { StartState: 'N; Edges: Map<'N, ('N * Edge<'C>) list>; EndStates: Set<'N> }

type IEdgeEvaluator<'C, 'X> =
    abstract member IsOnEdge: 'C * 'X -> bool

module FiniteStateMachine =
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
                            | Empty, _ when history |> Set.contains (node, inp) |> not ->
                                (inp, node, history |> Set.add (node, inp)) |> List.singleton
                            | WithToken c, x::xs when evaluator.IsOnEdge(c, x) && history |> Set.contains (node, xs) |> not ->
                                (xs, node, history |> Set.add (node, xs)) |> List.singleton
                            | _ ->
                                List.empty
                    )

                evaluateQueue (cs @ nextStates)

        evaluateQueue [(input, machine.StartState, Set.empty)]
