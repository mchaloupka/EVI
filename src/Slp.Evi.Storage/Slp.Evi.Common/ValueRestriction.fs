namespace Slp.Evi.Common

open System
open Slp.Fsm
open TCode.r2rml4net

module ValueRestriction =
    type TemplateFsmEdge =
        | AnyCharacter
        | IriUnRestrictedCharacter
        | DigitCharacter
        | ExactCharacter of char

    type TemplateFsm = FiniteStateMachine<GenericNode, TemplateFsmEdge>

    module TemplateFsm =
        let initialMachine = GenericNode.initialMachine

        let appendEdge = GenericNode.appendEdge

        let choiceMachine = GenericNode.choiceMachine

        let infiniteRepeatMachine = GenericNode.infiniteRepeatMachine

        let atLeastOneRepeatMachine = GenericNode.atLeastOneRepeatMachine

        let repeatMachine = GenericNode.repeatMachine

        let optionalMachine = GenericNode.optionalMachine

        let appendMachine machine appendTo =
            machine
            |> FiniteStateMachineBuilder.transformNodes GenericNode.transformNode
            |>  FiniteStateMachineBuilder.appendMachine <| appendTo

        let machineForCharacter c =
            initialMachine ()
            |> appendEdge (ExactCharacter c)

        let machineForText text =
            (initialMachine (), text)
            ||> Seq.fold (
                fun cur c ->
                    c |> ExactCharacter |> appendEdge <| cur
            )

        let finalizeMachine machine =
            machine
            |> FiniteStateMachine.removeLambdaEdges
            |> FiniteStateMachine.removeNonReachable

        let canAcceptSameText leftMachine rightMachine =
            let edgeIntersect le re =
                match le, re with
                | AnyCharacter, AnyCharacter ->
                    Some(AnyCharacter)
                | IriUnRestrictedCharacter, IriUnRestrictedCharacter ->
                    Some(IriUnRestrictedCharacter)
                | DigitCharacter, DigitCharacter ->
                    Some(DigitCharacter)
                | ExactCharacter a, ExactCharacter b ->
                    if a = b then
                        Some(ExactCharacter a)
                    else
                        None
                | AnyCharacter, IriUnRestrictedCharacter
                | IriUnRestrictedCharacter, AnyCharacter ->
                    Some(IriUnRestrictedCharacter)
                | AnyCharacter, DigitCharacter
                | DigitCharacter, AnyCharacter
                | IriUnRestrictedCharacter, DigitCharacter
                | DigitCharacter, IriUnRestrictedCharacter ->
                    Some(DigitCharacter)                    
                | AnyCharacter, ExactCharacter c
                | ExactCharacter c, AnyCharacter ->
                    Some(ExactCharacter c)
                | DigitCharacter, ExactCharacter c
                | ExactCharacter c, DigitCharacter ->
                    if Char.IsDigit c then
                        Some(ExactCharacter c)
                    else
                        None
                | IriUnRestrictedCharacter, ExactCharacter c
                | ExactCharacter c, IriUnRestrictedCharacter ->
                    if c |> MappingHelper.IsIUnreserved then
                        Some(ExactCharacter c)
                    else
                        None

            FiniteStateMachine.intersect edgeIntersect leftMachine rightMachine
            |> FiniteStateMachine.canAccept

        let accepts input machine =
            let edgeEvaluation (edge, c) =
                match edge with
                | AnyCharacter -> true
                | IriUnRestrictedCharacter -> c |> MappingHelper.IsIUnreserved
                | DigitCharacter -> c |> Char.IsDigit
                | ExactCharacter a -> a = c

            DeterministicFiniteStateMachine.accepts edgeEvaluation input machine

        let private getDisjunctParentEdge edge1 edge2 =
            match edge1, edge2 with
            | AnyCharacter, _
            | _, AnyCharacter ->
                AnyCharacter |> Some
            | IriUnRestrictedCharacter, DigitCharacter
            | DigitCharacter, IriUnRestrictedCharacter ->
                IriUnRestrictedCharacter |> Some
            | IriUnRestrictedCharacter, ExactCharacter c
            | ExactCharacter c, IriUnRestrictedCharacter when c |> MappingHelper.IsIUnreserved ->
                IriUnRestrictedCharacter |> Some
            | DigitCharacter, ExactCharacter c
            | ExactCharacter c, DigitCharacter when Char.IsDigit(c) ->
                DigitCharacter |> Some
            | IriUnRestrictedCharacter, IriUnRestrictedCharacter ->
                IriUnRestrictedCharacter |> Some
            | DigitCharacter, DigitCharacter ->
                DigitCharacter |> Some
            | ExactCharacter a, ExactCharacter b when a = b ->
                ExactCharacter a |> Some
            | _ ->
                None

        let private isNotDisjunct e1 e2 = getDisjunctParentEdge e1 e2 |> Option.isSome
        let private disjunctEdges edges =
            let rec processEdges toProcess output =
                match toProcess with
                | [] -> output
                | x :: xs -> processEdge x xs [] output
            and processEdge edge other output toProcess =
                match toProcess with
                | [] -> edge :: output |> processEdges other
                | x :: xs ->
                    match getDisjunctParentEdge edge x with
                    | Some e ->
                        e :: xs @ output |> processEdges other
                    | None ->
                        processEdge edge other (x :: output) xs

            processEdges edges []

        let compileMachine machine =
            DeterministicFiniteStateMachine.fromFiniteStateMachine disjunctEdges isNotDisjunct machine
            |> DeterministicFiniteStateMachine.transformNodes GenericNode.transformNode