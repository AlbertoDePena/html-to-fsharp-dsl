namespace FSharp.Html.Dsl

open System
open System.Collections.Generic

[<RequireQualifiedAccess>]
module Attr =
    /// XmlAttribute KeyValueAttr constructor. The value is HTML-escaped when
    /// rendered, so it is safe for user input.
    let create (key : string) (value : string) =
        KeyValueAttr (key, value)

    /// XmlAttribute RawKeyValueAttr constructor. The value is rendered
    /// verbatim - use only for values that are already escaped or that must
    /// contain raw markup. The `Attr.create`/`Attr.createRaw` pair mirrors
    /// `Text.enc`/`Text.raw`.
    let createRaw (key : string) (value : string) =
        RawKeyValueAttr (key, value)

    /// XmlAttribute NonValueAttr constructor
    let createBool (key : string) =
        NonValueAttr key

    let private additiveAttributes =
        dict [
            "class", " "
            "style", "; "
            "accept", ", " ]

    /// A merged attribute: its value (None for valueless attributes) and
    /// whether that value must be rendered verbatim.
    type private MergedValue =
        { Value : string option
          IsRaw : bool }

    /// Merge two XmlAttribute lists
    let merge (attrs1 : XmlAttribute list) (attrs2 : XmlAttribute list) =

        // An escaped half and a raw half cannot share one value, so when an
        // additive merge mixes them, escape the escaped half up front and
        // render the join verbatim.
        let joinValues (sep : string) (existing : MergedValue) (isRaw : bool) (value : string) =
            match existing.Value with
            | None ->
                // Pre-existing behaviour: a valueless attribute on the left
                // stays valueless and the incoming value is dropped.
                existing
            | Some existingValue when existing.IsRaw = isRaw ->
                { Value = Some (String.Concat [| existingValue; sep; value |]); IsRaw = isRaw }
            | Some existingValue ->
                let encode raw v = if raw then v else HtmlEncoding.encodeAttributeValue v
                let joined = String.Concat [| encode existing.IsRaw existingValue; sep; encode isRaw value |]
                { Value = Some joined; IsRaw = true }

        let readAttr attr =
            match attr with
            | NonValueAttr name             -> name, { Value = None;       IsRaw = false }
            | KeyValueAttr (name, value)    -> name, { Value = Some value; IsRaw = false }
            | RawKeyValueAttr (name, value) -> name, { Value = Some value; IsRaw = true }

        // convert left list into dictionary
        let merged = Dictionary<string, MergedValue>()

        for attr in attrs1 do
            let name, attrValue = readAttr attr
            merged.Add(name, attrValue)

        // check right list against dictionary, updating as appropriate
        for attr in attrs2 do
            let name, incoming = readAttr attr
            match incoming.Value with
            | None ->
                if not (merged.ContainsKey name) then
                    merged.Add(name, incoming)

            | Some value ->
                if merged.ContainsKey(name) && additiveAttributes.ContainsKey(name) then
                    merged[name] <- joinValues additiveAttributes[name] merged[name] incoming.IsRaw value
                elif merged.ContainsKey(name) then
                    merged[name] <- incoming
                else
                    merged.Add(name, incoming)

        // inputs are now merged, convert dictionary back into of XmlAttribute
        [
            for KeyValue (name, attrValue) in merged do
                match attrValue.Value with
                | None ->
                    NonValueAttr name
                | Some value when attrValue.IsRaw ->
                    RawKeyValueAttr (name, value)
                | Some value ->
                    KeyValueAttr (name, value)
        ]

    /// `accept={value}` attribute
    let accept = create "accept"

    /// `accept={value}` attribute
    let acceptCharset = create "accept-charset"

    /// `accesskey={value}` attribute
    let accesskey = create "accesskey"

    /// `action={value}` attribute
    let action = create "action"

    /// `align={value}` attribute
    let align = create "align"

    /// `allow={value}` attribute
    let allow = create "allow"

    /// `alt={value}` attribute
    let alt = create "alt"

    /// `aria-*={value}` attribute
    let aria name = create (String.Concat [ "aria-"; name ])

    /// `async` attribute
    let async = createBool "async"

    /// `autocapitalize={value}` attribute
    let autocapitalize = create "autocapitalize"

    /// `autocomplete={value}` attribute
    let autocomplete = create "autocomplete"

    /// `autofocus` attribute
    let autofocus = createBool "autofocus"

    /// `autoplay` attribute
    let autoplay = createBool "autoplay"

    /// `background={value}` attribute
    let background = create "background"

    /// `bgcolor={value}` attribute
    let bgcolor = create "bgcolor"

    /// `border={value}` attribute
    let border = create "border"

    /// `buffered={value}` attribute
    let buffered = create "buffered"

    /// `capture={value}` attribute
    let capture = create "capture"

    /// `challenge={value}` attribute
    let challenge = create "challenge"

    /// `charset={value}` attribute
    let charset = create "charset"

    /// `checked` attribute
    let checked' = createBool "checked"

    /// `cite={value}` attribute
    let cite = create "cite"

    /// `class={value}` attribute
    let class' = create "class"

    /// `code={value}` attribute
    let code = create "code"

    /// `codebase={value}` attribute
    let codebase = create "codebase"

    /// `color={value}` attribute
    let color = create "color"

    /// `cols={value}` attribute
    let cols = create "cols"

    /// `colspan={value}` attribute
    let colspan = create "colspan"

    /// `content={value}` attribute
    let content = create "content"

    /// `contenteditable={value}` attribute
    let contenteditable = create "contenteditable"

    /// `contextmenu={value}` attribute
    let contextmenu = create "contextmenu"

    /// `controls` attribute
    let controls = createBool "controls"

    /// `coords={value}` attribute
    let coords = create "coords"

    /// `crossorigin={value}` attribute
    let crossorigin = create "crossorigin"

    /// `csp={value}` attribute
    let csp = create "csp"

    /// `data-*={value}` attribute
    let data name = create (String.Concat [ "data-"; name ])

    /// `datetime={value}` attribute
    let datetime = create "datetime"

    /// `decoding={value}` attribute
    let decoding = create "decoding"

    /// `default` attribute
    let default' = createBool "default"

    /// `defer` attribute
    let defer = createBool "defer"

    /// `dir={value}` attribute
    let dir = create "dir"

    /// `dirname={value}` attribute
    let dirname = create "dirname"

    /// `disabled` attribute
    let disabled = createBool "disabled"

    /// `download={value}` attribute
    let download = create "download"

    /// `draggable={value}` attribute
    let draggable = create "draggable"

    /// `enctype={value}` attribute
    let enctype = create "enctype"

    /// `enterkeyhint={value}` attribute
    let enterkeyhint = create "enterkeyhint"

    /// `for={value}` attribute
    let for' = create "for"

    /// `form={value}` attribute
    let form = create "form"

    /// `formaction={value}` attribute
    let formaction = create "formaction"

    /// `formenctype={value}` attribute
    let formenctype = create "formenctype"

    /// `formmethod={value}` attribute
    let formmethod = create "formmethod"

    /// `formnovalidate` attribute
    let formnovalidate = createBool "formnovalidate"

    /// `formtarget={value}` attribute
    let formtarget = create "formtarget"

    /// `headers={value}` attribute
    let headers = create "headers"

    /// `height={value}` attribute
    let height = create "height"

    /// `hidden` attribute
    let hidden = createBool "hidden"

    /// `high={value}` attribute
    let high = create "high"

    /// `href={value}` attribute
    let href = create "href"

    /// `hreflang={value}` attribute
    let hreflang = create "hreflang"

    /// `http={value}` attribute
    let httpEquiv = create "http-equiv"

    /// `icon={value}` attribute
    let icon = create "icon"

    /// `id={value}` attribute
    let id = create "id"

    /// `importance={value}` attribute
    let importance = create "importance"

    /// `integrity={value}` attribute
    let integrity = create "integrity"

    /// `inputmode={value}` attribute
    let inputmode = create "inputmode"

    /// `ismap` attribute
    let ismap = createBool "ismap"

    /// `itemprop={value}` attribute
    let itemprop = create "itemprop"

    /// `keytype={value}` attribute
    let keytype = create "keytype"

    /// `kind={value}` attribute
    let kind = create "kind"

    /// `label={value}` attribute
    let label = create "label"

    /// `lang={value}` attribute
    let lang = create "lang"

    /// `loading={value}` attribute
    let loading = create "loading"

    /// `list={value}` attribute
    let list = create "list"

    /// `loop` attribute
    let loop = createBool "loop"

    /// `low={value}` attribute
    let low = create "low"

    /// `max={value}` attribute
    let max = create "max"

    /// `maxlength={value}` attribute
    let maxlength = create "maxlength"

    /// `minlength={value}` attribute
    let minlength = create "minlength"

    /// `media={value}` attribute
    let media = create "media"

    /// `method={value}` attribute
    let method = create "method"

    /// `min={value}` attribute
    let min = create "min"

    /// `multiple` attribute
    let multiple = createBool "multiple"

    /// `muted` attribute
    let muted = createBool "muted"

    /// `name={value}` attribute
    let name = create "name"

    /// `novalidate` attribute
    let novalidate = createBool "novalidate"

    /// `open={value}` attribute
    let open' = create "open"

    /// `optimum={value}` attribute
    let optimum = create "optimum"

    /// `pattern={value}` attribute
    let pattern = create "pattern"

    /// `ping={value}` attribute
    let ping = create "ping"

    /// `placeholder={value}` attribute
    let placeholder = create "placeholder"

    /// `poster={value}` attribute
    let poster = create "poster"

    /// `preload={value}` attribute
    let preload = create "preload"

    /// `radiogroup={value}` attribute
    let radiogroup = create "radiogroup"

    /// `readonly` attribute
    let readonly = createBool "readonly"

    /// `referrerpolicy={value}` attribute
    let referrerpolicy = create "referrerpolicy"

    /// `rel={value}` attribute
    let rel = create "rel"

    /// `required` attribute
    let required = createBool "required"

    /// `reversed` attribute
    let reversed = createBool "reversed"

    /// `role={value}` attribute
    let role = create "role"

    /// `rows={value}` attribute
    let rows = create "rows"

    /// `rowspan={value}` attribute
    let rowspan = create "rowspan"

    /// `sandbox={value}` attribute
    let sandbox = create "sandbox"

    /// `scope={value}` attribute
    let scope = create "scope"

    /// `selected` attribute
    let selected = createBool "selected"

    /// `shape={value}` attribute
    let shape = create "shape"

    /// `size={value}` attribute
    let size = create "size"

    /// `sizes={value}` attribute
    let sizes = create "sizes"

    /// `slot={value}` attribute
    let slot = create "slot"

    /// `span={value}` attribute
    let span = create "span"

    /// `spellcheck={value}` attribute
    let spellcheck = create "spellcheck"

    /// `src={value}` attribute
    let src = create "src"

    /// `srcdoc={value}` attribute
    let srcdoc = create "srcdoc"

    /// `srclang={value}` attribute
    let srclang = create "srclang"

    /// `srcset={value}` attribute
    let srcset = create "srcset"

    /// `start={value}` attribute
    let start = create "start"

    /// `step={value}` attribute
    let step = create "step"

    /// `style={value}` attribute
    let style = create "style"

    /// `tabindex={value}` attribute
    let tabindex = create "tabindex"

    /// `target={value}` attribute
    let target = create "target"

    /// `title={value}` attribute
    let title = create "title"

    /// `translate={value}` attribute
    let translate = create "translate"

    /// `type={value}` attribute
    let type' = create "type"

    /// `usemap={value}` attribute
    let usemap = create "usemap"

    /// `value={value}` attribute
    let value = create "value"

    /// `width={value}` attribute
    let width = create "width"

    /// `wrap={value}` attribute
    let wrap = create "wrap"


    //
    // Events

    /// `onabort={value}` event
    let onabort = create "onabort"

    /// `onafterprint={value}` event
    let onafterprint = create "onafterprint"

    /// `onanimationend={value}` event
    let onanimationend = create "onanimationend"

    /// `onanimationiteration={value}` event
    let onanimationiteration = create "onanimationiteration"

    /// `onanimationstart={value}` event
    let onanimationstart = create "onanimationstart"

    /// `onbeforeprint={value}` event
    let onbeforeprint = create "onbeforeprint"

    /// `onbeforeunload={value}` event
    let onbeforeunload = create "onbeforeunload"

    /// `onblur={value}` event
    let onblur = create "onblur"

    /// `oncanplay={value}` event
    let oncanplay = create "oncanplay"

    /// `oncanplaythrough={value}` event
    let oncanplaythrough = create "oncanplaythrough"

    /// `onchange={value}` event
    let onchange = create "onchange"

    /// `onclick={value}` event
    let onclick = create "onclick"

    /// `oncontextmenu={value}` event
    let oncontextmenu = create "oncontextmenu"

    /// `oncopy={value}` event
    let oncopy = create "oncopy"

    /// `oncut={value}` event
    let oncut = create "oncut"

    /// `ondblclick={value}` event
    let ondblclick = create "ondblclick"

    /// `ondrag={value}` event
    let ondrag = create "ondrag"

    /// `ondragend={value}` event
    let ondragend = create "ondragend"

    /// `ondragenter={value}` event
    let ondragenter = create "ondragenter"

    /// `ondragleave={value}` event
    let ondragleave = create "ondragleave"

    /// `ondragover={value}` event
    let ondragover = create "ondragover"

    /// `ondragstart={value}` event
    let ondragstart = create "ondragstart"

    /// `ondrop={value}` event
    let ondrop = create "ondrop"

    /// `ondurationchange={value}` event
    let ondurationchange = create "ondurationchange"

    /// `onemptied={value}` event
    let onemptied = create "onemptied"

    /// `onended={value}` event
    let onended = create "onended"

    /// `onerror={value}` event
    let onerror = create "onerror"

    /// `onfocus={value}` event
    let onfocus = create "onfocus"

    /// `onformchange={value}` event
    let onformchange = create "onformchange"

    /// `onforminput={value}` event
    let onforminput = create "onforminput"

    /// `onfocusin={value}` event
    let onfocusin = create "onfocusin"

    /// `onfocusout={value}` event
    let onfocusout = create "onfocusout"

    /// `onfullscreenchange={value}` event
    let onfullscreenchange = create "onfullscreenchange"

    /// `onfullscreenerror={value}` event
    let onfullscreenerror = create "onfullscreenerror"

    /// `onhashchange={value}` event
    let onhashchange = create "onhashchange"

    /// `oninput={value}` event
    let oninput = create "oninput"

    /// `oninvalid={value}` event
    let oninvalid = create "oninvalid"

    /// `onkeydown={value}` event
    let onkeydown = create "onkeydown"

    /// `onkeypress={value}` event
    let onkeypress = create "onkeypress"

    /// `onkeyup={value}` event
    let onkeyup = create "onkeyup"

    /// `onload={value}` event
    let onload = create "onload"

    /// `onloadeddata={value}` event
    let onloadeddata = create "onloadeddata"

    /// `onloadedmetadata={value}` event
    let onloadedmetadata = create "onloadedmetadata"

    /// `onloadstart={value}` event
    let onloadstart = create "onloadstart"

    /// `onmessage={value}` event
    let onmessage = create "onmessage"

    /// `onmousedown={value}` event
    let onmousedown = create "onmousedown"

    /// `onmouseenter={value}` event
    let onmouseenter = create "onmouseenter"

    /// `onmouseleave={value}` event
    let onmouseleave = create "onmouseleave"

    /// `onmousemove={value}` event
    let onmousemove = create "onmousemove"

    /// `onmouseover={value}` event
    let onmouseover = create "onmouseover"

    /// `onmouseout={value}` event
    let onmouseout = create "onmouseout"

    /// `onmouseup={value}` event
    let onmouseup = create "onmouseup"

    /// `onmousewheel={value}` event
    let onmousewheel = create "onmousewheel"

    /// `onoffline={value}` event
    let onoffline = create "onoffline"

    /// `ononline={value}` event
    let ononline = create "ononline"

    /// `onopen={value}` event
    let onopen = create "onopen"

    /// `onpagehide={value}` event
    let onpagehide = create "onpagehide"

    /// `onpageshow={value}` event
    let onpageshow = create "onpageshow"

    /// `onpaste={value}` event
    let onpaste = create "onpaste"

    /// `onpause={value}` event
    let onpause = create "onpause"

    /// `onplay={value}` event
    let onplay = create "onplay"

    /// `onplaying={value}` event
    let onplaying = create "onplaying"

    /// `onpopstate={value}` event
    let onpopstate = create "onpopstate"

    /// `onprogress={value}` event
    let onprogress = create "onprogress"

    /// `onratechange={value}` event
    let onratechange = create "onratechange"

    /// `onresize={value}` event
    let onresize = create "onresize"

    /// `onreset={value}` event
    let onreset = create "onreset"

    /// `onscroll={value}` event
    let onscroll = create "onscroll"

    /// `onsearch={value}` event
    let onsearch = create "onsearch"

    /// `onseeked={value}` event
    let onseeked = create "onseeked"

    /// `onseeking={value}` event
    let onseeking = create "onseeking"

    /// `onselect={value}` event
    let onselect = create "onselect"

    /// `onshow={value}` event
    let onshow = create "onshow"

    /// `onstalled={value}` event
    let onstalled = create "onstalled"

    /// `onstorage={value}` event
    let onstorage = create "onstorage"

    /// `onsubmit={value}` event
    let onsubmit = create "onsubmit"

    /// `onsuspend={value}` event
    let onsuspend = create "onsuspend"

    /// `ontimeupdate={value}` event
    let ontimeupdate = create "ontimeupdate"

    /// `ontoggle={value}` event
    let ontoggle = create "ontoggle"

    /// `ontouchcancel={value}` event
    let ontouchcancel = create "ontouchcancel"

    /// `ontouchend={value}` event
    let ontouchend = create "ontouchend"

    /// `ontouchmove={value}` event
    let ontouchmove = create "ontouchmove"

    /// `ontouchstart={value}` event
    let ontouchstart = create "ontouchstart"

    /// `ontransitionend={value}` event
    let ontransitionend = create "ontransitionend"

    /// `onunload={value}` event
    let onunload = create "onunload"

    /// `onvolumechange={value}` event
    let onvolumechange = create "onvolumechange"

    /// `onwaiting={value}` event
    let onwaiting = create "onwaiting"

    /// `onwheel={value}` event
    let onwheel = create "onwheel"
