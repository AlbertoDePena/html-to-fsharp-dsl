namespace FSharp.Html.Dsl.Tests

module AttrEncoding =

    open FSharp.Html.Dsl
    open FsUnit.Xunit
    open Xunit

    //
    // The attribute-unsafe characters

    [<Fact>]
    let ``Double quote in a value is escaped`` () =
        _input [ _value_ "a\"b" ]
        |> renderNode
        |> should equal """<input value="a&quot;b" />"""

    [<Fact>]
    let ``Ampersand in a value is escaped`` () =
        _a [ _href_ "/search?q=cats&page=2" ] []
        |> renderNode
        |> should equal """<a href="/search?q=cats&amp;page=2"></a>"""

    [<Fact>]
    let ``Single quote, less-than and greater-than in a value are escaped`` () =
        _div [ _title_ "it's <b>bold</b>" ] []
        |> renderNode
        |> should equal """<div title="it&#39;s &lt;b&gt;bold&lt;/b&gt;"></div>"""

    [<Fact>]
    let ``All unsafe characters are escaped in one value`` () =
        HtmlEncoding.encodeAttributeValue "&\"'<>"
        |> should equal "&amp;&quot;&#39;&lt;&gt;"

    //
    // The attack this escaping exists to stop

    [<Fact>]
    let ``A value cannot break out of its attribute to add an event handler`` () =
        // Pre-2.0.0 this rendered a live `onfocus` handler.
        let rendered = _input [ _value_ "\" onfocus=\"alert(1)" ] |> renderNode

        rendered |> should equal """<input value="&quot; onfocus=&quot;alert(1)" />"""
        rendered |> should not' (haveSubstring "onfocus=\"alert(1)\"")

    [<Fact>]
    let ``A value cannot break out of its attribute to add a new tag`` () =
        let rendered = _div [ _title_ "\"><script>alert(1)</script>" ] [] |> renderNode

        rendered |> should not' (haveSubstring "<script>")
        rendered |> should equal """<div title="&quot;&gt;&lt;script&gt;alert(1)&lt;/script&gt;"></div>"""

    [<Fact>]
    let ``Escaping survives attribute merging`` () =
        let attrs = Attr.merge [ _class_ "card" ] [ _class_ "\" onmouseover=\"alert(1)" ]

        _div attrs []
        |> renderNode
        |> should equal """<div class="card &quot; onmouseover=&quot;alert(1)"></div>"""

    //
    // Encoding is single-pass

    [<Fact>]
    let ``Encoding is single pass, so emitted entities are not re-escaped`` () =
        // The chained-Replace approach has to escape `&` first or `"` becomes
        // `&amp;quot;`. A single pass cannot make that mistake.
        HtmlEncoding.encodeAttributeValue "\"" |> should equal "&quot;"
        HtmlEncoding.encodeAttributeValue "<&>" |> should equal "&lt;&amp;&gt;"

    [<Fact>]
    let ``Encoding an already-encoded value double-encodes it`` () =
        // Documents the invariant: escape exactly once. Values that are already
        // escaped belong in `Attr.createRaw`, not `Attr.create`.
        HtmlEncoding.encodeAttributeValue "&amp;" |> should equal "&amp;amp;"

    //
    // Values that must pass through untouched

    [<Fact>]
    let ``A value needing no escaping is returned unchanged`` () =
        let value = "/products/42?page=1"
        HtmlEncoding.encodeAttributeValue value |> should be (sameAs value)

    [<Fact>]
    let ``Non-ASCII characters are not escaped`` () =
        // `WebUtility.HtmlEncode` would expand these into numeric entities and
        // bloat every UTF-8 document.
        _div [ _title_ "Привет мир こんにちは" ] []
        |> renderNode
        |> should equal """<div title="Привет мир こんにちは"></div>"""

    [<Fact>]
    let ``Empty and null values render as an empty attribute`` () =
        _input [ _value_ "" ] |> renderNode |> should equal """<input value="" />"""
        _input [ _value_ null ] |> renderNode |> should equal """<input value="" />"""

    //
    // Attr.createRaw escape hatch

    [<Fact>]
    let ``Attr.createRaw renders its value verbatim`` () =
        _input [ Attr.createRaw "value" "a\"b" ]
        |> renderNode
        |> should equal """<input value="a"b" />"""

    [<Fact>]
    let ``Attr.createRaw round-trips a pre-encoded value without double-encoding`` () =
        let encoded = HtmlEncoding.encodeAttributeValue "cats & dogs"

        _a [ Attr.createRaw "href" encoded ] []
        |> renderNode
        |> should equal """<a href="cats &amp; dogs"></a>"""

    [<Fact>]
    let ``renderFragment finds a raw id attribute`` () =
        let doc = _div [] [ _span [ Attr.createRaw "id" "target" ] [ _text "hit" ] ]

        renderFragment doc "target" |> should equal """<span id="target">hit</span>"""

    [<Fact>]
    let ``Merging a raw value with an escaped one escapes only the escaped half`` () =
        let attrs = Attr.merge [ Attr.createRaw "class" "a&amp;b" ] [ _class_ "c&d" ]

        _div attrs []
        |> renderNode
        |> should equal """<div class="a&amp;b c&amp;d"></div>"""

    [<Fact>]
    let ``Merging two raw values keeps both verbatim`` () =
        let attrs = Attr.merge [ Attr.createRaw "class" "a&amp;b" ] [ Attr.createRaw "class" "c&amp;d" ]

        _div attrs []
        |> renderNode
        |> should equal """<div class="a&amp;b c&amp;d"></div>"""

    //
    // Valueless attributes and attribute names

    [<Fact>]
    let ``Valueless attributes are unaffected`` () =
        _input [ _type_ "checkbox"; _checked_; _disabled_ ]
        |> renderNode
        |> should equal """<input type="checkbox" checked disabled />"""

    [<Fact>]
    let ``Attribute names are rendered verbatim, so keys must be literals`` () =
        // Documents a known limit: only values are escaped. A caller-supplied
        // key is still an injection vector, so keys must never be user input.
        Attr.create "data-x\" onload=\"alert(1)" "v"
        |> fun a -> _div [ a ] []
        |> renderNode
        |> should equal """<div data-x" onload="alert(1)="v"></div>"""
