namespace Slp.Evi.Common

open System

type Iri internal (uri: Uri) =
    do if uri.IsAbsoluteUri |> not then
            (sprintf "Provided URI is not absolute which is not allowed: %O" uri)
            |> invalidArg <| "uri"
            |> raise

    let iri = uri
    
    member internal _.Uri with get() = iri

    override _.ToString() = iri.AbsoluteUri

    override _.Equals(other) =
        match other with
        | :? Iri as i -> (iri, iri.Fragment) = (i.Uri, i.Uri.Fragment)
        | _ -> false

    override _.GetHashCode() =
        (iri, iri.Fragment).GetHashCode()

type IriReference =
    | AbsoluteIriReference of Iri
    | RelativeIriReference of string

module Iri =
    let fromUri (uri: Uri) =
        uri |> Iri

    let toUri (iri: Iri) =
        iri

    let toText (iri: Iri) =
        iri.Uri.AbsoluteUri

module IriReference =
    let fromUri (uri: Uri) =
        if uri.IsAbsoluteUri then
            uri |> Iri.fromUri |> AbsoluteIriReference
        else
            uri.ToString() |> RelativeIriReference
    
    let fromString (input: string) =
        input
        |> (fun x -> Uri(x, UriKind.RelativeOrAbsolute))
        |> fromUri

    let resolve (baseIri: Iri) (input: IriReference) =
        match input with
        | AbsoluteIriReference i -> i
        | RelativeIriReference r ->
            if r.Split('/') |> Seq.exists (fun x -> x = "." || x = "..") then
                (sprintf "Relative IRI cannot contain . or .. segments, but it was: %O" r)
                |> invalidArg <| "input"
                |> raise
            else
                (baseIri.Uri, r) |> Uri |> Iri.fromUri

    let tryResolve (baseIri: Iri option) (input: IriReference) =
        match (baseIri, input) with
        | None, AbsoluteIriReference iri -> iri
        | None, _ -> sprintf "Cannot resolve Iri reference %A without base Iri" input |> invalidArg "input"
        | Some b, i -> i |> resolve b
