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

    [<MemoryDiagnoser>]
    type RenderBench() =

        [<Benchmark>]
        member _.Falco() =
            products
            |> falcoTemplate
            |> renderNode

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<Markup.RenderBench>() |> ignore
    0 // return an integer exit code
