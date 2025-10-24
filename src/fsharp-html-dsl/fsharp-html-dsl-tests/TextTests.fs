namespace FSharp.Html.Dsl.Tests

module Text =

    open System
    open FSharp.Html.Dsl
    open FsUnit.Xunit
    open Xunit

    [<Fact>]
    let ``Text.empty should be empty`` () =
        renderNode Text.empty |> should equal String.Empty

    [<Fact>]
    let ``Text.raw should not be encoded`` () =
        let rawText = Text.raw "<div>"
        renderNode rawText |> should equal "<div>"

    [<Fact>]
    let ``Text.raw should not be encoded, but template applied`` () =
        let rawText = Text.rawf "<div>%s</div>" "fsharp"
        renderNode rawText |> should equal "<div>fsharp</div>"

    [<Fact>]
    let ``Text.enc should be encoded`` () =
        let encodedText = Text.enc "<div>"
        renderNode encodedText |> should equal "&lt;div&gt;"

    [<Fact>]
    let ``Text.comment should equal HTML comment`` () =
        let rawText = Text.comment "test comment"
        renderNode rawText |> should equal "<!-- test comment -->"

