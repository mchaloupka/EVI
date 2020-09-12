module Slp.Evi.Common.Iri.Tests

open Xunit
open System
open Slp.Evi.Common

[<Fact>]
let ``Absolute IRIs are equal if everything including fragment is equal`` () =
    (
        "http://example.com/segment#fragment" |> Uri |> Iri.fromUri,
        "http://example.com/segment#fragment" |> Uri |> Iri.fromUri
    )
    |> Assert.Equal

[<Fact>]
let ``Absolute IRIs are not equal if everything except fragment is equal`` () =
    (
        "http://example.com/segment#fragment" |> Uri |> Iri.fromUri,
        "http://example.com/segment#fragment2" |> Uri |> Iri.fromUri
    )
    |> Assert.NotEqual

[<Fact>]
let ``Combine works for rooted segments`` () =
    "/segment2" 
    |> IriReference.fromString
    |> IriReference.resolve ("http://example.com/segment/" |> Uri |> Iri.fromUri)
    |> fun x -> "http://example.com/segment2" |> Uri |> Iri.fromUri, x
    |> Assert.Equal

[<Fact>]
let ``Combine works for non-rooted segments`` () =
    "segment2" 
    |> IriReference.fromString
    |> IriReference.resolve ("http://example.com/segment/" |> Uri |> Iri.fromUri)
    |> fun x -> "http://example.com/segment/segment2" |> Uri |> Iri.fromUri, x
    |> Assert.Equal

[<Fact>]
let ``Combine works for fragments`` () =
    "#fragment" 
    |> IriReference.fromString
    |> IriReference.resolve ("http://example.com/segment" |> Uri |> Iri.fromUri)
    |> fun x -> "http://example.com/segment#fragment" |> Uri |> Iri.fromUri, x
    |> Assert.Equal
