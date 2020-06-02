module Slp.Evi.Storage.Core.Common.StringRestriction.Tests

open Xunit
open Slp.Evi.Common.StringRestriction

let intTemplate = [
    [ 
        [ExactCharacter '+']
        [ExactCharacter '-']
    ] |> Choice |> List.singleton |> Optional
    [ DigitCharacter ] |> AtLeastOneRepetition
]

let strTemplate = [
    [ AnyCharacter ] |> AtLeastOneRepetition
]

[<Fact>]
let ``a and b`` () =
    [ ExactCharacter 'a' ]
    |> RestrictedTemplate.canMatch
    <| [ ExactCharacter 'b' ]
    |> Assert.False

[<Fact>]
let ``a and a`` () =
    [ ExactCharacter 'a' ]
    |> RestrictedTemplate.canMatch
    <| [ ExactCharacter 'a' ]
    |> Assert.True

[<Fact>]
let ``. and a`` () =
    [ AnyCharacter ]
    |> RestrictedTemplate.canMatch
    <| [ ExactCharacter 'a' ]
    |> Assert.True

[<Fact>]
let ``\d and a`` () =
    [ DigitCharacter ]
    |> RestrictedTemplate.canMatch
    <| [ ExactCharacter 'a' ]
    |> Assert.False

[<Fact>]
let ``\d and 0`` () =
    [ DigitCharacter ]
    |> RestrictedTemplate.canMatch
    <| [ ExactCharacter '0' ]
    |> Assert.True

[<Fact>]
let ``{int} and 1234567`` () =
    intTemplate
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "1234567" 
    |> Assert.True

[<Fact>]
let ``{int} and 123a4567`` () =
    intTemplate
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "123a4567" 
    |> Assert.False

[<Fact>]
let ``{int} and +1234567`` () =
    intTemplate
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "+1234567" 
    |> Assert.True

[<Fact>]
let ``{int} and -1234567`` () =
    intTemplate
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "-1234567" 
    |> Assert.True

[<Fact>]
let ``text{int} text123`` () =
    [RestrictedTemplate.fromText "text"; intTemplate] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "text123" 
    |> Assert.True

[<Fact>]
let ``text{int} text`` () =
    [RestrictedTemplate.fromText "text"; intTemplate] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "text" 
    |> Assert.False

[<Fact>]
let ``{int}text text`` () =
    [ intTemplate; RestrictedTemplate.fromText "text" ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "text" 
    |> Assert.False

[<Fact>]
let ``{int}text 123text`` () =
    [ intTemplate; RestrictedTemplate.fromText "text" ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "123text" 
    |> Assert.True

[<Fact>]
let ``text{int}text text123text`` () =
    [ RestrictedTemplate.fromText "text"; intTemplate; RestrictedTemplate.fromText "text" ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "text123text" 
    |> Assert.True
[<Fact>]
let ``text{int}text texttext`` () =
    [ RestrictedTemplate.fromText "text"; intTemplate; RestrictedTemplate.fromText "text" ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "texttext" 
    |> Assert.False

[<Fact>]
let ``{int}text{int} 123text123`` () =
    [ intTemplate; RestrictedTemplate.fromText "text"; intTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "123text123" 
    |> Assert.True

[<Fact>]
let ``{int}tex{int} 123text123`` () =
    [ intTemplate; RestrictedTemplate.fromText "tex"; intTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "123text123" 
    |> Assert.False

[<Fact>]
let ``{str}text{str} 123text123`` () =
    [ strTemplate; RestrictedTemplate.fromText "text"; strTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "123text123" 
    |> Assert.True

[<Fact>]
let ``{str}tex{str} 123text123`` () =
    [ strTemplate; RestrictedTemplate.fromText "text"; strTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "123text123" 
    |> Assert.True

[<Fact>]
let ``{str}abc{str} 123text123`` () =
    [ strTemplate; RestrictedTemplate.fromText "text"; strTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| RestrictedTemplate.fromText "123text123" 
    |> Assert.True

[<Fact>]
let ``{str}abc{str} 123{str}123`` () =
    [ strTemplate; RestrictedTemplate.fromText "text"; strTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| ([ RestrictedTemplate.fromText "123"; strTemplate; RestrictedTemplate.fromText "123" ] |> List.concat)
    |> Assert.True

[<Fact>]
let ``{str}abc{str} {str}def{str}`` () =
    [ strTemplate; RestrictedTemplate.fromText "abc"; strTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| ([ strTemplate; RestrictedTemplate.fromText "def"; strTemplate ] |> List.concat)
    |> Assert.True

[<Fact(Skip="It is known that this scenario is not correctly identified.")>]
let ``{int}abc{int} {int}def{int}`` () =
    [ intTemplate; RestrictedTemplate.fromText "abc"; intTemplate ] |> List.concat
    |> RestrictedTemplate.canMatch
    <| ([ intTemplate; RestrictedTemplate.fromText "def"; intTemplate ] |> List.concat)
    |> Assert.False
