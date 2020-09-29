module Slp.Evi.Storage.Core.Relational.ConcatenationEqualityOptimizer.Tests

open Xunit

open Slp.Evi.Common.Types
open Slp.Evi.Common.Database
open Slp.Evi.Relational.Algebra
open Slp.Evi.Relational.ConcatenationEqualityOptimizer

let fakeSchema name = 
    {
        Name = name
        SqlType =
            { new ISqlColumnType with
                member _.DefaultRdfType = KnownTypes.xsdString
            }
        IsNullable = false
    }

let textPart = String >> Constant
let variable name = { Schema = fakeSchema name } |> Column
let col1 = variable "col1"
let col2 = variable "col2"
let col3 = variable "col3"
let col4 = variable "col4"

let assertAlwaysMatching result =
    result
    |> Option.get
    |> List.forall (
        function
        | AlwaysMatching -> true
        | _ -> false
    )
    |> Assert.True

let assertAlwaysNotMatching result =
    result
    |> Option.get
    |> List.exists (
        function
        | AlwaysNotMatching -> true
        | _ -> false
    )
    |> Assert.True

let assertIsEqualToExpected (expected: ConcatenationEqualityResult) result =
    Assert.Equal<ConcatenationEqualityResult>(expected, result |> Option.get)

[<Fact>]
let ``Test that equal textual concatenations always matches`` () =
    let template = "http://test.com" |> textPart |> List.singleton
    let result = compareConcatenations template template
    result |> assertAlwaysMatching

[<Fact>]
let ``Test that non-equal textual concatenations never matches with iri matching`` () =
    let leftTemplate = "http://test.com" |> textPart |> List.singleton
    let rightTemplate = "http://test.com/suffix" |> textPart |> List.singleton
    let result = compareConcatenations leftTemplate rightTemplate
    result |> assertAlwaysNotMatching

[<Fact>]
let ``Test that non-equal textual concatenations never matches without iri matching`` () =
    let leftTemplate = "http://test.com" |> textPart |> List.singleton
    let rightTemplate = "http://test.com/suffix" |> textPart |> List.singleton
    let result = compareConcatenations leftTemplate rightTemplate
    result |> assertAlwaysNotMatching

[<Fact>]
let ``Test that equivalent concatenations are reduced to column match with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col2 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    let expected =
        ([ col1 |> IriSafeVariable ], [ col2 |> IriSafeVariable ])
        |> MatchingCondition
        |> List.singleton

    result |> assertIsEqualToExpected expected

[<Fact>]
let ``Test that equivalent concatenations are reduced to column match without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> Variable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col2 |> Variable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    let expected =
        ([ col1 |> Variable ], [ col2 |> Variable ])
        |> MatchingCondition
        |> List.singleton

    result |> assertIsEqualToExpected expected

[<Fact>]
let ``Test that non equivalent in suffix concatenations never matches with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> IriSafeVariable
        "/suffix2" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col2 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    result |> assertAlwaysNotMatching

[<Fact>]
let ``Test that non equivalent in suffix concatenations never matches without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> Variable
        "/suffix2" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col2 |> Variable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    result |> assertAlwaysNotMatching

[<Fact>]
let ``Test that non equivalent in prefix concatenations never matches with iri matching`` () =
    let leftTemplate = [
        "https://test.com/" |> textPart
        col1 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col2 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    result |> assertAlwaysNotMatching

[<Fact>]
let ``Test that non equivalent in prefix concatenations never matches without iri matching`` () =
    let leftTemplate = [
        "https://test.com/" |> textPart
        col1 |> Variable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col2 |> Variable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    result |> assertAlwaysNotMatching

[<Fact>]
let ``Test that equivalent with more separated columns with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> IriSafeVariable
        "/" |> textPart
        col2 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col3 |> IriSafeVariable
        "/" |> textPart
        col4 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    let expected = [
        ([ col1 |> IriSafeVariable ], [ col3 |> IriSafeVariable ]) |> MatchingCondition
        ([ col2 |> IriSafeVariable ], [ col4 |> IriSafeVariable ]) |> MatchingCondition
    ]

    result |> assertIsEqualToExpected expected

[<Fact>]
let ``Test with more separated columns only on one side with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> IriSafeVariable
        "/" |> textPart
        col2 |> IriSafeVariable
        "/suffix2" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col3 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    result |> assertAlwaysNotMatching

[<Fact>]
let ``Test with non-separated columns only on one side without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> Variable
        "-" |> textPart
        col2 |> Variable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col3 |> Variable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    let expected = [
        (
            [
                col1 |> Variable
                "-" |> textPart
                col2 |> Variable
            ],[
                col3 |> Variable
            ]
        ) |> MatchingCondition
    ]

    result |> assertIsEqualToExpected expected

[<Fact>]
let ``Test with non-separated columns with multiple characters only on one side with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> IriSafeVariable
        "inner" |> textPart
        col2 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col3 |> IriSafeVariable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    let expected = [
        (
            [
                col1 |> IriSafeVariable
                "inner" |> textPart
                col2 |> IriSafeVariable
            ],[
                col3 |> IriSafeVariable
            ]
        ) |> MatchingCondition
    ]

    result |> assertIsEqualToExpected expected

[<Fact>]
let ``Test with non-separated columns with multiple characters only on one side without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> Variable
        "inner" |> textPart
        col2 |> Variable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col3 |> Variable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    let expected = [
        (
            [
                col1 |> Variable
                "inner" |> textPart
                col2 |> Variable
            ],[
                col3 |> Variable
            ]
        ) |> MatchingCondition
    ]

    result |> assertIsEqualToExpected expected

[<Fact>]
let ``Test with more separated columns only on one side without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> textPart
        col1 |> Variable
        "/" |> textPart
        col2 |> Variable
        "/suffix" |> textPart
    ]
    let rightTemplate = [
        "http://test.com/" |> textPart
        col3 |> Variable
        "/suffix" |> textPart
    ]
    let result = compareConcatenations leftTemplate rightTemplate
    let expected = [
        (
            [
                col1 |> Variable
                "/" |> textPart
                col2 |> Variable
            ],[
                col3 |> Variable
            ]
        ) |> MatchingCondition
    ]

    result |> assertIsEqualToExpected expected
