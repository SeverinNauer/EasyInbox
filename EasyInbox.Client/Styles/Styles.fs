module Styles

open Avalonia.Styling
open Avalonia.FuncUI.Types

let style (selector:Selector->Selector) (setters:IAttr<'a> seq) =
   let s = Style(fun x -> selector x )
   for attr in setters do
       match attr.Property with
       | Some p -> 
           match p.accessor with
           | InstanceProperty x -> failwith "Can't support instance property" 
           | AvaloniaProperty x -> s.Setters.Add(Setter(x,p.value))
       | None -> ()
   s