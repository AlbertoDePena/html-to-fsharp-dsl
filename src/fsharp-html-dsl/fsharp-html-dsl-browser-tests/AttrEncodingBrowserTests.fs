namespace FSharp.Html.Dsl.BrowserTests

open FSharp.Html.Dsl
open FsUnit.Xunit
open Xunit

/// End-to-end proof, in a real browser, that attribute escaping closes the
/// breakout hole and stays transparent to whoever reads the value back.
///
/// The unit tests assert the rendered string; these assert what Chromium
/// actually does with it - that no handler is created, that nothing executes,
/// and that the value a page reads back is byte-for-byte what was rendered in.
type AttrEncodingBrowserTests(fixture : BrowserFixture) =

    /// A value that terminates its own attribute and opens an event handler.
    /// This is what arrives when a query-string parameter is echoed into a
    /// `value=` attribute.
    let hostileValue = "\" onfocus=\"window.__xss = true"

    let page () =
        Skip.If(not (isNull fixture.SkipReason), fixture.SkipReason)
        fixture.NewPageAsync()

    interface IClassFixture<BrowserFixture>

    [<SkippableFact>]
    member _.``A hostile value cannot introduce an event handler``() =
        task {
            let! page = page ()

            let doc =
                _html [] [
                    _body [] [
                        _input [ _id_ "probe"; _type_ "text"; _value_ hostileValue ] ] ]

            do! page.SetContentAsync(renderHtml doc)

            // No stray attribute was parsed out of the value.
            let! hasHandler = page.EvalOnSelectorAsync<bool>("#probe", "el => el.hasAttribute('onfocus')")
            hasHandler |> should equal false

            let! attributeCount = page.EvalOnSelectorAsync<int>("#probe", "el => el.attributes.length")
            attributeCount |> should equal 3 // id, type, value

            // Triggering the event the payload aimed at runs nothing.
            do! page.FocusAsync "#probe"
            let! fired = page.EvaluateAsync<bool>("() => window.__xss === true")
            fired |> should equal false
        }

    [<SkippableFact>]
    member _.``Escaping is lossless - the browser reads back the original value``() =
        task {
            let! page = page ()

            let doc = _html [] [ _body [] [ _input [ _id_ "probe"; _value_ hostileValue ] ] ]
            do! page.SetContentAsync(renderHtml doc)

            // Entities are decoded by the parser, so the value is intact: the
            // escaping is a transport detail, not a mutation of the data.
            let! attributeValue = page.EvalOnSelectorAsync<string>("#probe", "el => el.getAttribute('value')")
            attributeValue |> should equal hostileValue

            let! liveValue = page.EvalOnSelectorAsync<string>("#probe", "el => el.value")
            liveValue |> should equal hostileValue
        }

    [<SkippableFact>]
    member _.``Attr.createRaw with a hostile value does break out``() =
        task {
            let! page = page ()

            // The regression anchor: this is exactly what every attribute did
            // before 2.0.0. If this test ever starts failing, `createRaw` has
            // stopped being raw.
            let doc = _html [] [ _body [] [ _input [ _id_ "probe"; Attr.createRaw "value" hostileValue ] ] ]
            do! page.SetContentAsync(renderHtml doc)

            let! hasHandler = page.EvalOnSelectorAsync<bool>("#probe", "el => el.hasAttribute('onfocus')")
            hasHandler |> should equal true

            do! page.FocusAsync "#probe"
            let! fired = page.EvaluateAsync<bool>("() => window.__xss === true")
            fired |> should equal true
        }

    [<SkippableFact>]
    member _.``Escaped quotes in an inline handler still execute as written``() =
        task {
            let! page = page ()

            // Renders as onclick="window.__result = &#39;a&quot;b&#39;" - the
            // parser hands the JS engine the original source.
            let doc =
                _html [] [
                    _body [] [
                        _button [ _id_ "btn"; _onclick_ "window.__result = 'a\"b'" ] [ _text "go" ] ] ]

            do! page.SetContentAsync(renderHtml doc)
            do! page.ClickAsync "#btn"

            let! result = page.EvaluateAsync<string>("() => window.__result")
            result |> should equal "a\"b"
        }

    [<SkippableFact>]
    member _.``An ampersand in a URL stays one URL with two parameters``() =
        task {
            let! page = page ()

            let doc =
                _html [] [
                    _body [] [
                        // Absolute, so `el.search` resolves - `SetContentAsync`
                        // leaves the page on `about:blank`, which cannot
                        // resolve a relative href.
                        _a [ _id_ "link"; _href_ "https://example.org/search?q=cats&page=2" ] [ _text "search" ] ] ]

            let rendered = renderHtml doc
            rendered |> should haveSubstring "href=\"https://example.org/search?q=cats&amp;page=2\""

            do! page.SetContentAsync(rendered)

            let! href = page.EvalOnSelectorAsync<string>("#link", "el => el.getAttribute('href')")
            href |> should equal "https://example.org/search?q=cats&page=2"

            // The browser parses it as a query string with both parameters,
            // which is what the `&amp;` is for.
            let! query = page.EvalOnSelectorAsync<string>("#link", "el => el.search")
            query |> should equal "?q=cats&page=2"
        }

    [<SkippableFact>]
    member _.``A fixed event attribute name actually binds a handler``() =
        task {
            let! page = page ()

            // `Attr.ontoggle` rendered `toggle="..."` before 2.0.0, which the
            // browser ignored entirely, so this handler never ran.
            let doc =
                _html [] [
                    _body [] [
                        _details [ _id_ "panel"; _ontoggle_ "window.__toggled = true" ] [
                            _summary [] [ _text "more" ]
                            _p [] [ _text "detail" ] ] ] ]

            do! page.SetContentAsync(renderHtml doc)
            do! page.ClickAsync "#panel summary"

            let! toggled = page.EvaluateAsync<bool>("() => window.__toggled === true")
            toggled |> should equal true
        }

    [<SkippableFact>]
    member _.``Non-ASCII attribute values survive the round trip``() =
        task {
            let! page = page ()

            let title = "Привет мир こんにちは & <friends>"
            let doc = _html [] [ _body [] [ _div [ _id_ "probe"; _title_ title ] [] ] ]

            do! page.SetContentAsync(renderHtml doc)

            let! readBack = page.EvalOnSelectorAsync<string>("#probe", "el => el.getAttribute('title')")
            readBack |> should equal title

            let! childCount = page.EvalOnSelectorAsync<int>("#probe", "el => el.children.length")
            childCount |> should equal 0 // `<friends>` did not become an element
        }
