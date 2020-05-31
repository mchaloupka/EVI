module Slp.Evi.Storage.Core.MappingTemplate.Tests

open Xunit
open Slp.Evi.R2RML.MappingTemplate

[<Fact>]
let ``Test that equal textual templates always matches with iri matching`` () =
    let template = "http://test.com" |> TextPart |> List.singleton
    let result = compareTemplates true template template
    result |> TemplateCompareResult.isAlwaysMatching |> Assert.True

[<Fact>]
let ``Test that equal textual templates always matches without iri matching`` () =
    let template = "http://test.com" |> TextPart |> List.singleton
    let result = compareTemplates false template template
    result |> TemplateCompareResult.isAlwaysMatching |> Assert.True

[<Fact>]
let ``Test that non-equal textual templates never matches with iri matching`` () =
    let leftTemplate = "http://test.com" |> TextPart |> List.singleton
    let rightTemplate = "http://test.com/suffix" |> TextPart |> List.singleton
    let result = compareTemplates true leftTemplate rightTemplate
    result |> TemplateCompareResult.isNeverMatching |> Assert.True

[<Fact>]
let ``Test that non-equal textual templates never matches without iri matching`` () =
    let leftTemplate = "http://test.com" |> TextPart |> List.singleton
    let rightTemplate = "http://test.com/suffix" |> TextPart |> List.singleton
    let result = compareTemplates false leftTemplate rightTemplate
    result |> TemplateCompareResult.isNeverMatching |> Assert.True

[<Fact>]
let ``Test that equivalent templates are reduced to column match with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates true leftTemplate rightTemplate
    let expected =
        ([ "col1" |> ColumnPart ], [ "col2" |> ColumnPart ])
        |> MatchingCondition
        |> List.singleton

    Assert.Equal<TemplateCompareResult<string>>(expected, result)

[<Fact>]
let ``Test that equivalent templates are reduced to column match without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates false leftTemplate rightTemplate
    let expected =
        ([ "col1" |> ColumnPart ], [ "col2" |> ColumnPart ])
        |> MatchingCondition
        |> List.singleton

    Assert.Equal<TemplateCompareResult<string>>(expected, result)

[<Fact>]
let ``Test that non equivalent in suffix templates never matches with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/suffix2" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates true leftTemplate rightTemplate
    result |> TemplateCompareResult.isNeverMatching |> Assert.True

[<Fact>]
let ``Test that non equivalent in suffix templates never matches without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/suffix2" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates false leftTemplate rightTemplate
    result |> TemplateCompareResult.isNeverMatching |> Assert.True

[<Fact>]
let ``Test that non equivalent in prefix templates never matches with iri matching`` () =
    let leftTemplate = [
        "https://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates true leftTemplate rightTemplate
    result |> TemplateCompareResult.isNeverMatching |> Assert.True

[<Fact>]
let ``Test that non equivalent in prefix templates never matches without iri matching`` () =
    let leftTemplate = [
        "https://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates false leftTemplate rightTemplate
    result |> TemplateCompareResult.isNeverMatching |> Assert.True

[<Fact>]
let ``Test that equivalent with more separated columns with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col3" |> ColumnPart
        "/" |> TextPart
        "col4" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates true leftTemplate rightTemplate
    let expected = [
        ([ "col1" |> ColumnPart ], [ "col3" |> ColumnPart ]) |> MatchingCondition
        ([ "col2" |> ColumnPart ], [ "col4" |> ColumnPart ]) |> MatchingCondition
    ]

    Assert.Equal<TemplateCompareResult<string>>(expected, result)

[<Fact>]
let ``Test with more separated columns only on one side with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/" |> TextPart
        "col2" |> ColumnPart
        "/suffix2" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col3" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates true leftTemplate rightTemplate
    result |> TemplateCompareResult.isNeverMatching |> Assert.True

[<Fact>]
let ``Test with non-separated columns only on one side without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "-" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col3" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates false leftTemplate rightTemplate
    let expected = [
        (
            [
                "col1" |> ColumnPart
                "-" |> TextPart
                "col2" |> ColumnPart
            ],[
                "col3" |> ColumnPart
            ]
        ) |> MatchingCondition
    ]

    Assert.Equal<TemplateCompareResult<string>>(expected, result)

[<Fact>]
let ``Test with non-separated columns with multiple characters only on one side with iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "inner" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col3" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates true leftTemplate rightTemplate
    let expected = [
        (
            [
                "col1" |> ColumnPart
                "inner" |> TextPart
                "col2" |> ColumnPart
            ],[
                "col3" |> ColumnPart
            ]
        ) |> MatchingCondition
    ]

    Assert.Equal<TemplateCompareResult<string>>(expected, result)

[<Fact>]
let ``Test with non-separated columns with multiple characters only on one side without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "inner" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col3" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates false leftTemplate rightTemplate
    let expected = [
        (
            [
                "col1" |> ColumnPart
                "inner" |> TextPart
                "col2" |> ColumnPart
            ],[
                "col3" |> ColumnPart
            ]
        ) |> MatchingCondition
    ]

    Assert.Equal<TemplateCompareResult<string>>(expected, result)

[<Fact>]
let ``Test with more separated columns only on one side without iri matching`` () =
    let leftTemplate = [
        "http://test.com/" |> TextPart
        "col1" |> ColumnPart
        "/" |> TextPart
        "col2" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let rightTemplate = [
        "http://test.com/" |> TextPart
        "col3" |> ColumnPart
        "/suffix" |> TextPart
    ]
    let result = compareTemplates false leftTemplate rightTemplate
    let expected = [
        (
            [
                "col1" |> ColumnPart
                "/" |> TextPart
                "col2" |> ColumnPart
            ],[
                "col3" |> ColumnPart
            ]
        ) |> MatchingCondition
    ]

    Assert.Equal<TemplateCompareResult<string>>(expected, result)
