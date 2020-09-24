module ControlStyles
open Avalonia.Styling
open Styles
open Avalonia.Controls
open Avalonia.Controls.Presenters
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.DSL

let listBoxItemSelected = 
    style <|
    (fun s -> s.OfType<ListBoxItem>().Class(":selected").Template().OfType<ContentPresenter>()) <|
    [ContentPresenter.background "#414244"]


let allStyles = [listBoxItemSelected]