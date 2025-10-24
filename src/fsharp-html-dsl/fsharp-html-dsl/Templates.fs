namespace FSharp.Html.Dsl

[<RequireQualifiedAccess>]
module Templates =    
    /// SVG Version 1.0 template with customizable viewBox width/height
    let svg (x : int, y : int, w : int, h : int) (content : XmlNode list) =
        Elem.svg [
            Svg.Attr.viewBox (sprintf "%i %i %i %i" x y w h)
            Svg.Attr.xmlns "http://www.w3.org/2000/svg"
        ] content
