# FSharp.Html.Dsl

```fsharp
open FSharp.Html.Dsl

let doc =
    _html [] [
        _body [ _class_ "100-vh" ] [
            _h1 [] [ _text "Hello world!" ] ] ]

renderHtml doc
```

`FSharp.Html.Dsl` is an XML markup module that can be used to produce any form of angle-bracket markup (i.e. HTML, SVG, XML etc.).

## Installation

### Install the package

`dotnet add package FSharp.Html.Dsl`

### In F# Project

```xml
<PackageReference Include="FSharp.Html.Dsl" Version="x.x.x" />
```

## Key Features

- Use native F# to produce any form of angle-bracket markup.
- Simple to create reusable blocks of code (i.e., partial views and components).
- Easily extended by creating custom tags and attributes.
- Compiled as part of your assembly, leading to improved performance and simpler deployments.
- Provides strongly typed functions matching the full HTML [spec](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference).

## Design Goals

- Provide a tool to generate any form of angle-bracket markup.
- Must be performant and memory efficient.
- Should be simple, extensible and integrate with existing .NET libraries.
- Can be easily learned.
- Match HTML [spec](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference) as closely as possible.
- Support rendering full documents as well as fragments.

## Overview

`FSharp.Html.Dsl` is broken down into three primary modules, `Elem`, `Attr` and `Text`, which are used to generate elements, attributes and text nodes respectively. Each module contain a suite of functions mapping to the various element/attribute/node names. But can also be extended to create custom elements and attributes.

Primary elements are broken down into two types, `ParentNode` or `SelfClosingNode`.

`ParentNode` elements are those that can contain other elements. Represented as functions that receive two inputs: attributes and optionally elements.

Each of the primary modules can be access using the name directly, or using the "underscore syntax" seen below.

| Module | Syntax |
|--------|--------|
| `Elem` | `_h1 [] []` |
| `Attr` | `_class_ "my-class"` |
| `Text` | `_text "Hello world!"` |


```fsharp
let markup =
    _div [ _class_ "heading" ] [
        _h1 [] [ _text "Hello world!" ] ]
```

`SelfClosingNode` elements are self-closing tags. Represented as functions that receive one input: attributes.

```fsharp
let markup =
    _div [ _class_ "divider" ] [
        _hr [] ]
```

Text is represented using the `TextNode` and created using one of the functions in the `Text` module.

```fsharp
let markup =
    _div [] [
        _p [] [ _text "A paragraph" ]
        _p [] [ _textf "Hello %s" "Jim" ]
        _code [] [ _textEnc "<div>Hello</div>" ] // HTML encodes text before rendering
    ]
```

Attributes contain two subtypes as well, `KeyValueAttr` which represent key/value attributes or `NonValueAttr` which represent boolean attributes.

```fsharp
let markup =
    _input [ _type_ "text"; _required_ ]
```

Most [JavaScript Events](https://developer.mozilla.org/en-US/docs/Web/Events) have also been mapped in the `Attr` module. All of these events are prefixed with the word "on" (i.e., `_onclick_`, `_onfocus_` etc.)

```fsharp
let markup =
    _button [ _onclick_ "console.log(\"hello world\")" ] [ _text "Click me" ]
```

## HTML

Though `FSharp.Html.Dsl` can be used to produce any markup. It is first and foremost an HTML library.

### Combining views to create complex output

```fsharp
open FSharp.Html.Dsl

// Components
let divider =
    _hr [ _class_ "divider" ]

// Template
let master (title : string) (content : XmlNode list) =
    _html [ _lang_ "en" ] [
        _head [] [
            _title [] [ _text title ]
        ]
        _body [] content
    ]

// Views
let homeView =
    master "Homepage" [
        _h1 [] [ _text "Homepage" ]
        divider
        _p [] [ _text "Lorem ipsum dolor sit amet, consectetur adipiscing." ]
    ]

let aboutView =
    master "About Us" [
        _h1 [] [ _text "About" ]
        divider
        _p [] [ _text "Lorem ipsum dolor sit amet, consectetur adipiscing." ]
    ]
```

### Strongly-typed views

```fsharp
open FSharp.Html.Dsl

type Person =
    { FirstName : string
      LastName : string }

let doc (person : Person) =
    _html [ _lang_ "en" ] [
        _head [] [
            _title [] [ _text "Sample App" ]
        ]
        _body [] [
            _main [] [
                _h1 [] [ _text "Sample App" ]
                _p [] [ _text $"{person.First} {person.Last}" ]
            ]
        ]
    ]
```

### Merging Attributes

The markup module allows you to easily create components, an excellent way to reduce code repetition in your UI. To support runtime customization, it is advisable to ensure components (or reusable markup blocks) retain a similar function "shape" to standard elements. That being, `XmlAttribute list -> XmlNode list -> XmlNode`.

This means that you will inevitably end up needing to combine your predefined `XmlAttribute list` with a list provided at runtime. To facilitate this, the `Attr.merge` function will group attributes by key, and intelligently concatenate the values in the case of additive attributes (i.e., `class`, `style` and `accept`).

```fsharp
open FSharp.Html.Dsl

// Components
let heading (attrs : XmlAttribute list) (content : XmlNode list) =
    // safely combine the default XmlAttribute list with those provided
    // at runtime
    let attrs' =
        Attr.merge [ _class_ "text-large" ] attrs

    _div [] [
        _h1 [ attrs' ] content
    ]

// Template
let master (title : string) (content : XmlNode list) =
    _html [ _lang_ "en" ] [
        _head [] [
            _title [] [ _text title ]
        ]
        _body [] content
    ]

// Views
let homepage =
    master "Homepage" [
        heading [ _class_ "red" ] [ _text "Welcome to the homepage" ]
        _p [] [ _text "Lorem ipsum dolor sit amet, consectetur adipiscing." ]
    ]

let homepage =
    master "About Us" [
        heading [ _class_ "purple" ] [ _text "This is what we're all about" ]
        _p [] [ _text "Lorem ipsum dolor sit amet, consectetur adipiscing." ]
    ]
```

## Custom Elements & Attributes

Every effort has been taken to ensure the HTML and SVG specs are mapped to functions in the module. In the event an element or attribute you need is missing, you can either file an [issue](https://github.com/AlbertoDePena/html-to-fsharp-dsl/issues), or more simply extend the module in your project.

An example creating custom XML elements and using them to create a structured XML document:

```fsharp
open FSharp.Html.Dsl

module XmlElem =
    let books = Attr.create "books"
    let book = Attr.create "book"
    let name = Attr.create "name"

module XmlAttr =
    let soldOut = Attr.createBool "soldOut"

let xmlDoc =
    XmlElem.books [] [
        XmlElem.book [ XmlAttr.soldOut ] [
            XmlElem.name [] [ _text "To Kill A Mockingbird" ]
        ]
    ]

let xml = renderXml xmlDoc
```

## Template Fragments

There are circumstances where you may want to render only a portion of your view. Especially common in [hypermedia driven](https://htmx.org/essays/hypermedia-driven-applications/) applications. Supporting [template fragments](https://htmx.org/essays/template-fragments/) is helpful in maintaining locality of behavior, because it allows you to decompose a particular view for partial updates internally without pulling fragments of the template out to separate files for rendering, creating a large number of individual templates.

`FSharp.Html.Dsl` supports this pattern by way of the `renderFragment` function, which will traverse the provided `XmlNode` tree and render only the child node matching the provided `id`. Otherwise, gracefully returning an empty string if no match is found.

```fsharp
open FSharp.Html.Dsl

let view =
    _div [ _id_ "my-div"; _class_ "my-class" ] [
        _h1 [ _id_ "my-heading" ] [ _text "hello" ] ]

let render = renderFragment doc "my-heading"
// produces: <h1 id="my-heading">hello</h1>
```

## SVG

Much of the SVG spec has been mapped to element and attributes functions. There is also an SVG template to help initialize a new drawing with a valid viewbox.

```fsharp
open FSharp.Html.Dsl
open FSharp.Html.Dsl.Svg

// https://developer.mozilla.org/en-US/docs/Web/SVG/Element/text#example
let svgDrawing =
    Templates.svg (0, 0, 240, 80) [
        _style [] [
            _text ".small { font: italic 13px sans-serif; }"
            _text ".heavy { font: bold 30px sans-serif; }"
            _text ".Rrrrr { font: italic 40px serif; fill: red; }"
        ]
        _text [ _x_ "20"; _y_ "35"; _class_ "small" ] [ _text "My" ]
        _text [ _x_ "40"; _y_ "35"; _class_ "heavy" ] [ _text "cat" ]
        _text [ _x_ "55"; _y_ "55"; _class_ "small" ] [ _text "is" ]
        _text [ _x_ "65"; _y_ "55"; _class_ "Rrrrr" ] [ _text "Grumpy!" ]
    ]

let svg = renderNode svgDrawing
```

## Performance

```shell
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.2604 (21H2)
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET SDK=7.0.201
  [Host]     : .NET 6.0.14 (6.0.1423.7309), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.14 (6.0.1423.7309), X64 RyuJIT


|        Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Allocated |
|-------------- |----------:|----------:|----------:|------:|--------:|--------:|----------:|
|        FSharp |  3.829 us | 0.0338 us | 0.0300 us |  1.58 |    0.04 |  8.1253 |     17 KB |
```

## Development

### Building Locally

```bash
# Build and test everything
dotnet test -c Release

# Or step by step:
dotnet build -c Release                     # Build all projects
dotnet test -c Release                      # Run tests
dotnet pack src/fsharp-html-dsl -c Release  # Create package

# Run benchmarks
dotnet run --project src/benchmarks/Benchmarks.fsproj -c Release
```
