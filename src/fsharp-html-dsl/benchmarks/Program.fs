open System
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open FSharp.Html.Dsl

module Markup =    

    type Product =
        { Name : string
          Price : float
          Description : string }

    let lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

    let products =
        [ 1..5 ]
        |> List.map (fun i -> { Name = sprintf "Name %i" i; Price = i |> float; Description = lorem})

    let falcoTemplate products =
        let elem product =
            Elem.li [] [
                Elem.h2 [] [ Text.raw product.Name ]
                Text.rawf "Only %f" product.Price
                Text.raw product.Description ]

        products
        |> List.map elem
        |> Elem.ul [ Attr.id "products" ]

    /// Attribute-heavy rows: the shape that exercises attribute-value
    /// escaping, which the product template above barely touches.
    let attrTemplate (values : string list) =
        let row i value =
            Elem.tr [ Attr.class' "row"; Attr.data "index" (string i) ] [
                Elem.td [] [
                    Elem.input [
                        Attr.type' "text"
                        Attr.name (sprintf "field-%i" i)
                        Attr.value value
                        Attr.create "hx-get" (sprintf "/rows/%i?q=%s&page=2" i value)
                        Attr.create "aria-label" value ] ] ]

        values
        |> List.mapi row
        |> Elem.table [ Attr.id "rows" ]

    /// No character needs an entity - the path that should not allocate.
    let cleanValues = [ for i in 1..50 -> sprintf "product-name-%i" i ]

    /// Every value needs escaping.
    let dirtyValues = [ for i in 1..50 -> sprintf "a \"quoted\" & <escaped> value %i" i ]

    [<MemoryDiagnoser>]
    type RenderBench() =

        [<Benchmark>]
        member _.Falco() =
            products
            |> falcoTemplate
            |> renderNode

        [<Benchmark>]
        member _.AttributesNoEscaping() =
            cleanValues
            |> attrTemplate
            |> renderNode

        [<Benchmark>]
        member _.AttributesWithEscaping() =
            dirtyValues
            |> attrTemplate
            |> renderNode

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<Markup.RenderBench>() |> ignore
    0 // return an integer exit code
