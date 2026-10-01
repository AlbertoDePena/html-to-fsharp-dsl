namespace FSharp.Html.Dsl

open System
open System.Buffers
open System.Globalization
open System.IO
open System.Text

/// Specifies an XML-style attribute
type XmlAttribute =
    /// Key/value attribute whose value is HTML-escaped when rendered. This is
    /// what `Attr.create` and every named `Attr.*`/`_name_` function produce,
    /// so values taken from user input are safe by default.
    | KeyValueAttr of string * string
    /// Key/value attribute whose value is rendered verbatim. Only for values
    /// that are already escaped, or that must contain raw markup.
    | RawKeyValueAttr of string * string
    /// Valueless attribute, e.g. `autofocus`
    | NonValueAttr of string

/// Represents an XML-style element containing attributes
type XmlElement =
    string * XmlAttribute list

/// Describes the different XML-style node patterns
type XmlNode =
    | TextNode        of string
    | SelfClosingNode of XmlElement
    | ParentNode      of XmlElement * XmlNode list

/// HTML-escaping for attribute values.
///
/// Attribute values are always emitted inside double quotes, so an unescaped
/// `"` in a value terminates it early and anything after it is parsed as
/// further attributes: `Attr.value "\" onfocus=alert(1) x=\""` would otherwise
/// render as a live event handler. `Text.enc` covers text nodes; this covers
/// the attribute side.
[<RequireQualifiedAccess>]
module HtmlEncoding =

    /// The characters that cannot appear literally inside a double-quoted
    /// attribute value.
    ///
    /// `"` is what actually terminates the value. `'`, `<` and `>` are escaped
    /// too so the output stays safe if it is ever re-quoted with single quotes
    /// or read by a lenient parser. Every other character - non-ASCII
    /// included - is written through unchanged, which keeps UTF-8 documents
    /// UTF-8 instead of expanding them into numeric entities.
    ///
    /// `SearchValues` gives a vectorised `IndexOfAny`, so scanning a value that
    /// needs no escaping - the common case - costs close to nothing.
    let private unsafeChars = SearchValues.Create("&\"'<>".AsSpan())

    /// The entity for one of `unsafeChars`. Only ever reached for a character
    /// that `IndexOfAny unsafeChars` matched, so `>` takes the default arm.
    let inline private entityFor (c : char) =
        match c with
        | '&'  -> "&amp;"
        | '"'  -> "&quot;"
        | '\'' -> "&#39;"
        | '<'  -> "&lt;"
        | _    -> "&gt;"

    /// Writes `value` to `w`, replacing attribute-unsafe characters with
    /// entities.
    ///
    /// Single pass, so an entity it emits is never re-escaped - unlike chained
    /// `String.Replace` calls, where `&` has to be handled first to avoid
    /// turning `&quot;` into `&amp;quot;`. A value needing no escaping is
    /// written straight through in one call, and the runs between entities are
    /// written as spans, so nothing is allocated either way.
    let writeAttributeValue (w : TextWriter) (value : string) =
        if not (String.IsNullOrEmpty value) then
            let span = value.AsSpan()
            let mutable next = span.IndexOfAny unsafeChars

            if next < 0 then
                w.Write value
            else
                let mutable start = 0

                while next >= 0 do
                    if next > 0 then
                        w.Write(span.Slice(start, next))

                    w.Write(entityFor span[start + next])
                    start <- start + next + 1
                    next <- span.Slice(start).IndexOfAny unsafeChars

                if start < span.Length then
                    w.Write(span.Slice start)

    /// `writeAttributeValue` as a string function. Returns the input unchanged
    /// when nothing needs escaping. Rendering does not need this - the
    /// serializer escapes on the way out - but it is useful when a value has to
    /// be escaped ahead of `Attr.createRaw`.
    let encodeAttributeValue (value : string) =
        if String.IsNullOrEmpty value || value.AsSpan().IndexOfAny unsafeChars < 0 then
            value
        else
            let sb = StringBuilder(value.Length + 16)
            use w = new StringWriter(sb, CultureInfo.InvariantCulture)
            writeAttributeValue w value
            sb.ToString()

[<AbstractClass; Sealed>]
type StringBuilderCache internal() =
    // The value 360 was chosen in discussion with performance experts as a compromise between using
    // as litle memory (per thread) as possible and still covering a large part of short-lived
    // StringBuilder creations on the startup path of VS designers.
    [<Literal>]
    static let _maxBuilderSize = 360

    // == StringBuilder.DefaultCapacity
    [<Literal>]
    static let _defaultCapacity = 16

    [<ThreadStatic; DefaultValue>]
    static val mutable private cachedInstance : StringBuilder

    static member Acquire (?capacity : int) =
        let capacity' = defaultArg capacity _defaultCapacity
        let sb = StringBuilderCache.cachedInstance

        // Avoid stringbuilder block fragmentation by getting a new StringBuilder
        // when the requested size is larger than the current capacity
        if capacity' <= _maxBuilderSize &&
           not(isNull sb) &&
           capacity' <= sb.Capacity then
           StringBuilderCache.cachedInstance <- null
           sb.Clear() |> ignore
           sb
        else
            new StringBuilder(capacity')

    static member GetString (sb : StringBuilder) =
        let result = sb.ToString()
        if sb.Capacity <= _maxBuilderSize then StringBuilderCache.cachedInstance <- sb
        result

module internal XmlNodeSerializer =
    let [<Literal>] _openChar = '<'
    let [<Literal>] _closeChar = '>'
    let [<Literal>] _term = '/'
    let [<Literal>] _space = ' '
    let [<Literal>] _equals = '='
    let [<Literal>] _quote = '"'

    let serialize (w : TextWriter, xml : XmlNode) =
        let writeAttributes attrs =
            for attr in (attrs : XmlAttribute list) do
                w.Write _space

                match attr with
                | NonValueAttr attrName ->
                    w.Write attrName

                | KeyValueAttr (attrName, attrValue) ->
                    w.Write attrName
                    w.Write _equals
                    w.Write _quote
                    HtmlEncoding.writeAttributeValue w attrValue
                    w.Write _quote

                | RawKeyValueAttr (attrName, attrValue) ->
                    w.Write attrName
                    w.Write _equals
                    w.Write _quote
                    w.Write attrValue
                    w.Write _quote

        let rec buildXml tag =
            match tag with
            | TextNode text ->
                w.Write text

            | SelfClosingNode (tag, attrs) ->
                w.Write _openChar
                w.Write tag
                writeAttributes attrs
                w.Write _space
                w.Write _term
                w.Write _closeChar

            | ParentNode ((tag, attrs), children) ->
                w.Write _openChar
                w.Write tag
                writeAttributes attrs
                w.Write _closeChar

                for c in children do
                    buildXml c

                w.Write _openChar
                w.Write _term
                w.Write tag
                w.Write _closeChar

        buildXml xml

[<AutoOpen>]
module XmlNodeRenderer =
    let private render (tag : XmlNode) (header : string option) =
        let sb = StringBuilderCache.Acquire()
        let w = new StringWriter(sb, CultureInfo.InvariantCulture)
        match header with
        | Some x -> w.Write x
        | None   -> ()
        XmlNodeSerializer.serialize(w, tag)
        StringBuilderCache.GetString(w.GetStringBuilder())

    /// Render XmlNode as string
    let renderNode (tag : XmlNode) =
        render tag None

    /// Render XmlNode as HTML string
    let renderHtml (tag : XmlNode) =
        render tag (Some "<!DOCTYPE html>")

    /// Render XmlNode as XML string
    let renderXml (tag : XmlNode) =
        render tag (Some "<?xml version=\"1.0\" encoding=\"UTF-8\"?>")

    /// Render a fragment of XmlNode by id as string
    let renderFragment (tag : XmlNode) (id : string) =
        let isIdMatch attr =
            match attr with
            | KeyValueAttr ("id", v)
            | RawKeyValueAttr ("id", v) when v = id -> true
            | _ -> false

        let rec findId tag =
            match tag with
            | TextNode _ ->
                None

            | SelfClosingNode ((_, attrs)) ->
                attrs
                |> List.tryFind isIdMatch
                |> Option.map (fun _ -> tag)

            | ParentNode ((_, attrs), children) ->
                attrs
                |> List.tryFind isIdMatch
                |> Option.map (fun _ -> tag)
                |> Option.orElse (children |> List.tryPick findId)

        match findId tag with
        | Some node -> render node None
        | None      -> String.Empty
