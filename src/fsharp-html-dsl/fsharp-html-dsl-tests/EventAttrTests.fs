namespace FSharp.Html.Dsl.Tests

module EventAttr =

    open FSharp.Html.Dsl
    open FsUnit.Xunit
    open Xunit

    /// Before 2.0.0, 31 of these helpers dropped the `on` prefix from the
    /// rendered attribute name (`Attr.onwheel` produced `wheel="..."`), which
    /// silently produced a dead attribute. These assert the rendered name, not
    /// just that the `_name_` alias matches the `Attr.*` function.
    let private rendersAs (attr : XmlAttribute) (expectedName : string) =
        _div [ attr ] []
        |> renderNode
        |> should equal (sprintf "<div %s=\"handler\"></div>" expectedName)

    [<Fact>]
    let ``Every event attribute renders with its on-prefixed name`` () =
        rendersAs (Attr.onabort "handler") "onabort"
        rendersAs (Attr.onafterprint "handler") "onafterprint"
        rendersAs (Attr.onanimationend "handler") "onanimationend"
        rendersAs (Attr.onanimationiteration "handler") "onanimationiteration"
        rendersAs (Attr.onanimationstart "handler") "onanimationstart"
        rendersAs (Attr.onbeforeprint "handler") "onbeforeprint"
        rendersAs (Attr.onbeforeunload "handler") "onbeforeunload"
        rendersAs (Attr.onblur "handler") "onblur"
        rendersAs (Attr.oncanplay "handler") "oncanplay"
        rendersAs (Attr.oncanplaythrough "handler") "oncanplaythrough"
        rendersAs (Attr.onchange "handler") "onchange"
        rendersAs (Attr.onclick "handler") "onclick"
        rendersAs (Attr.oncontextmenu "handler") "oncontextmenu"
        rendersAs (Attr.oncopy "handler") "oncopy"
        rendersAs (Attr.oncut "handler") "oncut"
        rendersAs (Attr.ondblclick "handler") "ondblclick"
        rendersAs (Attr.ondrag "handler") "ondrag"
        rendersAs (Attr.ondragend "handler") "ondragend"
        rendersAs (Attr.ondragenter "handler") "ondragenter"
        rendersAs (Attr.ondragleave "handler") "ondragleave"
        rendersAs (Attr.ondragover "handler") "ondragover"
        rendersAs (Attr.ondragstart "handler") "ondragstart"
        rendersAs (Attr.ondrop "handler") "ondrop"
        rendersAs (Attr.ondurationchange "handler") "ondurationchange"
        rendersAs (Attr.onemptied "handler") "onemptied"
        rendersAs (Attr.onended "handler") "onended"
        rendersAs (Attr.onerror "handler") "onerror"
        rendersAs (Attr.onfocus "handler") "onfocus"
        rendersAs (Attr.onformchange "handler") "onformchange"
        rendersAs (Attr.onforminput "handler") "onforminput"
        rendersAs (Attr.onfocusin "handler") "onfocusin"
        rendersAs (Attr.onfocusout "handler") "onfocusout"
        rendersAs (Attr.onfullscreenchange "handler") "onfullscreenchange"
        rendersAs (Attr.onfullscreenerror "handler") "onfullscreenerror"
        rendersAs (Attr.onhashchange "handler") "onhashchange"
        rendersAs (Attr.oninput "handler") "oninput"
        rendersAs (Attr.oninvalid "handler") "oninvalid"
        rendersAs (Attr.onkeydown "handler") "onkeydown"
        rendersAs (Attr.onkeypress "handler") "onkeypress"
        rendersAs (Attr.onkeyup "handler") "onkeyup"
        rendersAs (Attr.onload "handler") "onload"
        rendersAs (Attr.onloadeddata "handler") "onloadeddata"
        rendersAs (Attr.onloadedmetadata "handler") "onloadedmetadata"
        rendersAs (Attr.onloadstart "handler") "onloadstart"
        rendersAs (Attr.onmessage "handler") "onmessage"
        rendersAs (Attr.onmousedown "handler") "onmousedown"
        rendersAs (Attr.onmouseenter "handler") "onmouseenter"
        rendersAs (Attr.onmouseleave "handler") "onmouseleave"
        rendersAs (Attr.onmousemove "handler") "onmousemove"
        rendersAs (Attr.onmouseover "handler") "onmouseover"
        rendersAs (Attr.onmouseout "handler") "onmouseout"
        rendersAs (Attr.onmouseup "handler") "onmouseup"
        rendersAs (Attr.onmousewheel "handler") "onmousewheel"
        rendersAs (Attr.onoffline "handler") "onoffline"
        rendersAs (Attr.ononline "handler") "ononline"
        rendersAs (Attr.onopen "handler") "onopen"
        rendersAs (Attr.onpagehide "handler") "onpagehide"
        rendersAs (Attr.onpageshow "handler") "onpageshow"
        rendersAs (Attr.onpaste "handler") "onpaste"
        rendersAs (Attr.onpause "handler") "onpause"
        rendersAs (Attr.onplay "handler") "onplay"
        rendersAs (Attr.onplaying "handler") "onplaying"
        rendersAs (Attr.onpopstate "handler") "onpopstate"
        rendersAs (Attr.onprogress "handler") "onprogress"
        rendersAs (Attr.onratechange "handler") "onratechange"
        rendersAs (Attr.onresize "handler") "onresize"
        rendersAs (Attr.onreset "handler") "onreset"
        rendersAs (Attr.onscroll "handler") "onscroll"
        rendersAs (Attr.onsearch "handler") "onsearch"
        rendersAs (Attr.onseeked "handler") "onseeked"
        rendersAs (Attr.onseeking "handler") "onseeking"
        rendersAs (Attr.onselect "handler") "onselect"
        rendersAs (Attr.onshow "handler") "onshow"
        rendersAs (Attr.onstalled "handler") "onstalled"
        rendersAs (Attr.onstorage "handler") "onstorage"
        rendersAs (Attr.onsubmit "handler") "onsubmit"
        rendersAs (Attr.onsuspend "handler") "onsuspend"
        rendersAs (Attr.ontimeupdate "handler") "ontimeupdate"
        rendersAs (Attr.ontoggle "handler") "ontoggle"
        rendersAs (Attr.ontouchcancel "handler") "ontouchcancel"
        rendersAs (Attr.ontouchend "handler") "ontouchend"
        rendersAs (Attr.ontouchmove "handler") "ontouchmove"
        rendersAs (Attr.ontouchstart "handler") "ontouchstart"
        rendersAs (Attr.ontransitionend "handler") "ontransitionend"
        rendersAs (Attr.onunload "handler") "onunload"
        rendersAs (Attr.onvolumechange "handler") "onvolumechange"
        rendersAs (Attr.onwaiting "handler") "onwaiting"
        rendersAs (Attr.onwheel "handler") "onwheel"

    [<Fact>]
    let ``Event attribute values are escaped`` () =
        _button [ _onclick_ """alert("hi")""" ] []
        |> renderNode
        |> should equal """<button onclick="alert(&quot;hi&quot;)"></button>"""
